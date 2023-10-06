using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MRADS2
{
    /// <summary>
    /// An interface that allows for importing of data from a source
    /// </summary>
    public interface ImportSource
    {
        void OpenFile(string filename);
        void Close();

        string Filename { get; }

        DateTime Start { get; }
        DateTime End { get; }

        void GotoStart();
        CANMessage ReadNext();

        void Invalidate()
        {
        }
    }

    /// <summary>
    /// A class to import data from a specified source and write that data to a file in the mradsdb format
    /// </summary>
    public class MRADSDataImporter
    {
        const int StatusUpdateRate = 100; // ms

        public int MinSamplePeriod = 100; // ms
        public int MaxTimeBetweenMessages = 1000; // ms

        // A hardcoded list of CAN source addresses used to filter messages that are being received
        // more frequently than the MinSamplePeriod value. This should definitely not be hardcoded like this
        public byte[] MinSamplePeriodSources = new byte[] { 0xe2 };

        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }

        public enum ImportStatus { Searching, Working, Done }

        public Action<ImportStatus, double> ProgressCallback = null;
        public Func<bool> CancelCallback = null;

        ImportSource source;
        MRADSShip ship;

        public MRADSDataImporter(ImportSource source, MRADSShip ship)
        {
            this.source = source;
            this.ship = ship;

            StartTime = source.Start;
            StopTime = source.End;
        }

        public void WriteFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);
            CANMessage msg = null;
            MsgEntry entry = null;
            List<MRADSEvent> events = new List<MRADSEvent>();
            Dictionary<uint, (DateTime dt, byte[] data)> CANValues = new Dictionary<uint, (DateTime, byte[])>();
            int i;
            bool firstmsg = true;
            ImportStatus status = ImportStatus.Searching;
            DateTime laststatusupdate = DateTime.Now;

            ship.State.Reset();

            var sectiontable = new SectionTable(new SectionTable.SectionEntry(SectionTable.SectionIDs.Messages), new SectionTable.SectionEntry(SectionTable.SectionIDs.Events));
            var header = new Header((ushort)sectiontable.Size);

            fs.SetLength(Header.Size);
            fs.Position = fs.Length;

            var msgsection = sectiontable[SectionTable.SectionIDs.Messages];
            var eventsection = sectiontable[SectionTable.SectionIDs.Events];

            source.GotoStart();

            while (true)
            {
                if (msg != null && msg.Time > StopTime)
                    break;

                msg = source.ReadNext();
                if (msg == null)
                    break;

                // Perform a progress callback if it exists and the appropriate amount of time has passed since the previous one
                if (ProgressCallback != null && (DateTime.Now - laststatusupdate).TotalMilliseconds > StatusUpdateRate)
                {
                    double progress;

                    if (status == ImportStatus.Searching)
                        progress = (msg.Time - source.Start).Ticks / (double)(source.End - source.Start).Ticks;
                    else
                        progress = (msg.Time - StartTime).Ticks / (double)(StopTime - StartTime).Ticks;

                    ProgressCallback(status, progress);

                    if (CancelCallback != null)
                    {
                        // Check for a cancel
                        if (CancelCallback())
                        {
                            fs.Close();

                            File.Delete(fs.Name);

                            ProgressCallback(ImportStatus.Done, 100);

                            source.Invalidate();

                            return;
                        }
                    }

                    laststatusupdate = DateTime.Now;
                }

                // Check if the start time has been reached yet
                if (status == ImportStatus.Searching)
                {
                    if (msg.Time < StartTime)
                        continue;
                    else
                        status = ImportStatus.Working;
                }

                entry = new MsgEntry(msg);

                if (firstmsg)
                {
                    events.AddRange(ship.CheckEvents(MRADSShip.MessageType.First, entry));

                    firstmsg = false;
                }

                // Check if this message triggers any events
                var evt = CheckEvents(entry);
                if (evt != null)
                    events.AddRange(evt);

                // Create a unique identifier using the CAN channel and the CAN ID
                var chanid = (uint)((msg.Channel << 29) | (msg.Priority << 24) | (msg.PGN << 8) | msg.Source);

                // Filter out messages that match certain criteria:
                // - If the message generated an event then always record it
                // - If the message is coming from a source in the MinSamplePeriodSources array and it hasn't been
                //   at least MinSamplePeriod ms since the previous messages then drop it
                // - If the data in a message hasn't changed from the previous message and it has been less than
                //   MaxTimeBetweenMessages since the previous recorded message then drop it
                if ((evt == null || evt.Count == 0) && CANValues.ContainsKey(chanid))
                {
                    var msdelta = (msg.Time - CANValues[chanid].dt).TotalMilliseconds;

                    if (MinSamplePeriodSources.Contains(msg.Source) && msdelta < MinSamplePeriod)
                        continue;
                    else if (msdelta < MaxTimeBetweenMessages)
                    {
                        for (i = 0; i < msg.Data.Length; i++)
                        {
                            if (msg.Data[i] != CANValues[chanid].data[i])
                                break;
                        }

                        if (i == msg.Data.Length)
                            continue;
                    }
                }

                CANValues[chanid] = (msg.Time, msg.Data);

                var bin = entry.Serialize();

                fs.Write(bin);

                msgsection.Length += (ulong)bin.Length;
            }

            events.AddRange(ship.CheckEvents(MRADSShip.MessageType.Last, entry));

            // Sort events by time and serialize them
            var eventdata = events.OrderBy(a => a.Time).Select(a => BinarySerialize(a)).SelectMany(a => a).ToArray();

            msgsection.Location = (ulong)Header.Size;
            header.SectionTableLocation = msgsection.Location + msgsection.Length;
            eventsection.Location = header.SectionTableLocation + (ulong)sectiontable.Size;
            eventsection.Length = (ulong)eventdata.Length;

            fs.Write(BinarySerialize(sectiontable));
            fs.Write(eventdata);
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(BinarySerialize(header));

            fs.Close();

            source.Invalidate();

            ProgressCallback(ImportStatus.Done, 100);
        }

        List<MRADSEvent> CheckEvents(MsgEntry msg)
        {
            var provider = ship.Config.GetProvider(msg.Source, msg.Channel, msg.PGN);
            if (provider == null)
                return (null);

            var decoder = provider.GetDecoder(msg.PGN, msg.Source);
            if (decoder == null)
                return (null);

            var values = decoder.Decode(msg.Data);

            if (values.Count == 0)
                return (null);

            var changes = new MRADSDataChangeSet(provider, decoder, msg.Time, null, values);
            
            ship.State.UpdateState(changes);

            return (ship.CheckEvents(MRADSShip.MessageType.Standard, msg, changes));
        }

        /// <summary>
        /// Serialize an object to a byte array using reflection
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        static byte[] BinarySerialize(object o)
        {
            List<byte> ret;
            var type = o.GetType();

            if (type.IsValueType)
            {
                if (type.IsEnum)
                    return (BinarySerialize(Convert.ChangeType(o, ((Enum)o).GetTypeCode())));
                else if (o is byte || o is char)
                    return (new byte[] { (byte)Convert.ChangeType(o, typeof(byte)) });
                else
                    return (BitConverter.GetBytes((dynamic)o));
            }
            else if (o is string)
                return (BitConverter.GetBytes((ushort)((string)o).Length).Concat(Encoding.ASCII.GetBytes((string)o)).ToArray());

            ret = new List<byte>();

            foreach (var p in type.GetProperties())
            {
                if (p.GetCustomAttribute<DontSerialize>() != null)
                    continue;

                var v = p.GetValue(o);

                if (v is Array)
                {
                    var attr = p.GetCustomAttribute<SerializeArrayLength>();

                    if (attr == null || attr.VariableLength)
                        ret.AddRange(BinarySerialize((ushort)((Array)v).Length));

                    foreach (var v1 in (Array)v)
                        ret.AddRange(BinarySerialize(v1));
                }
                else
                    ret.AddRange(BinarySerialize(v));
            }

            return (ret.ToArray());
        }

        static readonly Dictionary<Type, int> TypeLength = new Dictionary<Type, int>()
        {
            { typeof(byte), 1 },
            { typeof(char), 1 },
            { typeof(ushort), 2 },
            { typeof(short), 2 },
            { typeof(uint), 4 },
            { typeof(int), 4 },
            { typeof(ulong), 8 },
            { typeof(long), 8 }
        };

        static T ByteConverter<T>(byte[] data, int index)
        {
            ulong v = 0;
            int i, sz = TypeLength[typeof(T)];

            for (i = 0; i < sz; i++)
                v |= (ulong)data[i + index] << (i * 8);

            return ((T)Convert.ChangeType(v, typeof(T)));
        }

        static public T BinaryDeserialize<T>(byte[] buf)
        {
            int index = 0;

            return (BinaryDeserialize<T>(buf, ref index));
        }

        /// <summary>
        /// Deserialize a given type from the given byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buf"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static public T BinaryDeserialize<T>(byte[] buf, ref int index)
        {
            object[] args;
            object v;
            var type = typeof(T);
            T ret;

            if (type.IsValueType)
            {
                if (type.IsEnum)
                    type = type.GetEnumUnderlyingType();

                int len = TypeLength[type];

                // Call the generic byte converter method using the type parameter to convert the data
                var val = typeof(MRADSDataImporter).GetMethod("ByteConverter", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(type).Invoke(null, new object[] { buf, index });
                index += len;

                return ((T)val);
            }
            else if (type == typeof(string))
            {
                // Strings are stored with a 2 byte length followed by the string data
                int len = BitConverter.ToUInt16(buf, index);
                index += 2;

                var str = Encoding.ASCII.GetString(buf, index, len);

                index += len;

                return ((T)(object)str);
            }
            
            // Instantiate the type
            ret = (T)Activator.CreateInstance(type);

            // Iterate through the public properties, recursively deserializing the values
            foreach (var p in type.GetProperties())
            {
                if (p.GetCustomAttribute<DontSerialize>() != null)
                    continue;

                if (p.PropertyType.IsArray)
                {
                    var attr = p.GetCustomAttribute<SerializeArrayLength>();
                    int len;

                    if (attr == null || attr.VariableLength)
                    {
                        // Variable length arrays store their length as a 2 byte value
                        len = BitConverter.ToUInt16(buf, index);
                        index += 2;
                    }
                    else
                        len = attr.ArrayLength;

                    var array = Array.CreateInstance(p.PropertyType.GetElementType(), len);

                    for (int i = 0; i < len; i++)
                    {
                        args = new object[] { buf, index };
                        v = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(p.PropertyType.GetElementType()).Invoke(null, args);
                        index = (int)args[1];

                        array.SetValue(v, i);
                    }

                    p.SetValue(ret, array);
                }
                else
                {
                    args = new object[] { buf, index };
                    v = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(p.PropertyType).Invoke(null, args);
                    index = (int)args[1];

                    p.SetValue(ret, v);
                }
            }

            return (ret);
        }

        public class DontSerialize : Attribute
        {
        }

        public class SerializeArrayLength : Attribute
        {
            public bool VariableLength => ArrayLength == -1;
            public int ArrayLength { get; set; } = -1;
        }

        /// <summary>
        /// The mradsdb header
        /// </summary>
        public class Header
        {
            const ushort CurrentVersion = 1;

            readonly static char[] IDValue = new char[] { 'M', 'R', 'A', 'D', 'S' };

            [SerializeArrayLength(ArrayLength = 5)]
            public char[] ID { get; set; }
            public ushort Version { get; set; }

            public ulong SectionTableLocation { get; set; }
            public ulong SectionTableLength { get; set; }

            [DontSerialize]
            public static int Size => IDValue.Length + 2 + 8 + 8;

            public Header()
            {
            }

            public Header(ulong sectiontablelength)
            {
                ID = IDValue;
                Version = CurrentVersion;
                SectionTableLength = sectiontablelength;
            }

            public static Header Deserialize(FileStream fs)
            {
                byte[] buf = new byte[Size];
                int i;
                Header ret;

                int len = fs.Read(buf, 0, buf.Length);
                if (len < buf.Length)
                    throw new Exception("Invalid header length");

                ret = BinaryDeserialize<Header>(buf);

                for (i = 0; i < IDValue.Length; i++)
                {
                    if (ret.ID[i] != IDValue[i])
                        break;
                }

                if (i != IDValue.Length)
                    throw new Exception("Invalid header");

                return (ret);
            }
        }

        public class SectionTable
        {
            public enum SectionIDs : byte { Messages = 0, Events = 1 };

            public class SectionEntry
            {
                public SectionIDs SectionID { get; set; }
                public ulong Location { get; set; }
                public ulong Length { get; set; }

                [DontSerialize]
                public static int Size => 1 + 8 + 8;

                public SectionEntry()
                {
                }

                public SectionEntry(SectionIDs sectionid)
                {
                    SectionID = sectionid;
                    Location = 0;
                    Length = 0;
                }
            }

            public SectionEntry[] Sections { get; set; }

            [DontSerialize]
            public int Size => SectionEntry.Size * Sections.Length + 2; // Additional two bytes to encode the length of the array

            public SectionTable()
            {
            }

            public SectionTable(params SectionEntry[] sections)
            {
                Sections = sections;
            }

            [DontSerialize]
            public SectionEntry this[SectionIDs id] => Sections.SingleOrDefault(a => a.SectionID == id);

            public static SectionTable Deserialize(FileStream fs, Header hdr)
            {
                byte[] buf = new byte[hdr.SectionTableLength];
                SectionTable ret;

                fs.Position = (long)hdr.SectionTableLocation;

                int len = fs.Read(buf, 0, buf.Length);
                if (len < buf.Length)
                    throw new Exception("Invalid section table length");

                ret = BinaryDeserialize<SectionTable>(buf);

                return (ret);
            }
        }

        /// <summary>
        /// A CAN message mradsdb entry
        /// </summary>
        public class MsgEntry : ICANMessage
        {
            public long TimeTicks => Time.Ticks;
            public uint Chan_ID => (uint)((Channel << 29) | (Priority << 24) | (PGN << 8) | Source);
            [SerializeArrayLength(ArrayLength = 8)]
            public byte[] Data { get; private set; }

            [DontSerialize]
            public DateTime Time { get; private set; }

            [DontSerialize]
            public int Channel { get; private set; }

            [DontSerialize]
            public byte Priority { get; private set; }

            [DontSerialize]
            public ushort PGN { get; private set; }

            [DontSerialize]
            public byte Source { get; private set; }

            [DontSerialize]
            public static int Size => 8 + 4 + 8;

            public double TimeStamp => throw new NotImplementedException();

            public MsgEntry(CANMessage msg)
            {
                Time = msg.Time;

                Channel = msg.Channel;
                Priority = msg.Priority;
                PGN = msg.PGN;
                Source = msg.Source;

                Data = (byte[])msg.Data.Clone();
            }

            MsgEntry()
            {
            }

            public static MsgEntry Deserialize(byte[] data, int index = 0)
            {
                int idx = index;
                uint chanid;
                MsgEntry ret = new MsgEntry();

                ret.Time = new DateTime(BitConverter.ToInt64(data, index));
                idx += 8;

                chanid = BitConverter.ToUInt32(data, idx);
                idx += 4;

                ret.Channel = (int)(chanid >> 29);
                ret.Priority = (byte)((chanid >> 24) & 0x3f);
                ret.PGN = (ushort)(chanid >> 8);
                ret.Source = (byte)chanid;

                ret.Data = new byte[8];
                Array.Copy(data, idx, ret.Data, 0, ret.Data.Length);

                return (ret);
            }

            public byte[] Serialize()
            {
                byte[] ret = new byte[Size];
                int idx = 0;

                Array.Copy(BitConverter.GetBytes(TimeTicks), ret, 8);
                idx += 8;

                Array.Copy(BitConverter.GetBytes(Chan_ID), 0, ret, idx, 4);
                idx += 4;

                Array.Copy(Data, 0, ret, idx, 8);

                return (ret);
            }
        }

        /// <summary>
        /// Write the CAN messages from the source to a CSV file
        /// </summary>
        /// <param name="filename"></param>
        public void WriteFileCANCSV(string filename)
        {
            CANMessage msg = null;
            ImportStatus status = ImportStatus.Searching;
            DateTime laststatusupdate = DateTime.Now;

            StreamWriter sw = new StreamWriter(filename);

            source.GotoStart();

            while (true)
            {
                if (msg != null && msg.Time > StopTime)
                    break;

                msg = source.ReadNext();
                if (msg == null)
                    break;

                if (ProgressCallback != null && (DateTime.Now - laststatusupdate).TotalMilliseconds > StatusUpdateRate)
                {
                    double progress;

                    if (status == ImportStatus.Searching)
                        progress = (msg.Time - source.Start).Ticks / (double)(source.End - source.Start).Ticks;
                    else
                        progress = (msg.Time - StartTime).Ticks / (double)(StopTime - StartTime).Ticks;

                    ProgressCallback(status, progress);

                    if (CancelCallback != null)
                    {
                        if (CancelCallback())
                        {
                            sw.Close();

                            File.Delete(filename);

                            ProgressCallback(ImportStatus.Done, 100);

                            source.Invalidate();

                            return;
                        }
                    }

                    laststatusupdate = DateTime.Now;
                }

                if (status == ImportStatus.Searching)
                {
                    if (msg.Time < StartTime)
                        continue;
                    else
                        status = ImportStatus.Working;
                }

                sw.WriteLine(msg.ToCSV());
            }

            sw.Close();

            source.Invalidate();

            ProgressCallback(ImportStatus.Done, 100);
        }

        /// <summary>
        /// Write the data from the specified data source to a CSV file. All variable values
        /// are written at each time point so this file can get very large very quickly
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="exportsource"></param>
        public void WriteFileCSV(string filename, MRADSDataSource exportsource)
        {
            List<MRADSDataChangeSet> changes = null;
            int changeidx;
            ImportStatus status = ImportStatus.Working;
            DateTime laststatusupdate = DateTime.Now;
            MRADSShipState state = ship.State.Clone();

            StreamWriter sw = new StreamWriter(filename);

            sw.WriteLine("Time," + state.GetAllVariables().Select(a => a.Provider.Name + ": " + a.Value.VariableDefinition.Name).Aggregate((a, b) => a + "," + b));

            var initialstart = exportsource.BufferStart;
            var initialend = exportsource.BufferEnd;

            changes = exportsource.GetChangesAtTime(StartTime);
            changeidx = 0;

            while (changes[changeidx].Time < StopTime)
            {
                state.UpdateState(changes[changeidx++]);
                sw.WriteLine(state.Time.ToString("yyyy-MM-dd HH:mm:ss.fff") + "," + state.GetAllVariables().Select(a => a.Value.Set ? a.Value.GetValueObj().ToString() : "").Aggregate((a, b) => a + "," + b));

                if (changeidx == changes.Count)
                {
                    exportsource.GetChangesAtTime(exportsource.BufferEnd);

                    changeidx = 0;
                }

                if (ProgressCallback != null && (DateTime.Now - laststatusupdate).TotalMilliseconds > StatusUpdateRate)
                {
                    double progress;

                    if (status == ImportStatus.Searching)
                        progress = (state.Time - exportsource.Start).Ticks / (double)(exportsource.End - exportsource.Start).Ticks;
                    else
                        progress = (state.Time - StartTime).Ticks / (double)(StopTime - StartTime).Ticks;

                    ProgressCallback(status, progress);

                    if (CancelCallback != null)
                    {
                        if (CancelCallback())
                        {
                            sw.Close();

                            exportsource.SetBuffer(initialstart, initialend);

                            ProgressCallback(ImportStatus.Done, 100);

                            return;
                        }
                    }

                    laststatusupdate = DateTime.Now;
                }
            }

            sw.Close();

            exportsource.SetBuffer(initialstart, initialend);

            ProgressCallback(ImportStatus.Done, 100);
        }
    }
}

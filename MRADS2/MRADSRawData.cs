using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRADS2
{
    public abstract class MRADSRawData
    {
        abstract public string Filename { get; }
        abstract public DateTime Start { get; }
        abstract public DateTime End { get; }

        abstract public object Position { get; set; }

        abstract public void GotoTime(DateTime time);
        abstract public void GotoStart();
        abstract public void GotoEnd();
        abstract public CANMessage ReadNext();
        abstract public CANMessage ReadPrev();

        abstract public void Close();

        public List<CANMessage> ReadCANMessages(DateTime start, DateTime end, IEnumerable<ushort> PGNs)
        {
            List<CANMessage> ret = new List<CANMessage>();
            CANMessage msg;

            GotoTime(start);

            while ((msg = ReadNext()) != null)
            {
                if (msg.Time > end)
                    break;
                else if (PGNs.Contains(msg.PGN))
                    ret.Add(msg);
            }

            return (ret);
        }

        public Dictionary<ushort, CANMessage> FindCANMessagePGNs(DateTime start, IEnumerable<ushort> PGNS)
        {
            Dictionary<ushort, CANMessage> ret = new Dictionary<ushort, CANMessage>();
            CANMessage msg;

            foreach (var PGN in PGNS)
                ret.Add(PGN, null);

            GotoTime(start);

            while ((msg = ReadNext()) != null)
            {
                if (ret.ContainsKey(msg.PGN) && ret[msg.PGN] != null)
                {
                    ret[msg.PGN] = msg;

                    if (ret.Values.All(a => a != null))
                        break;
                }
            }

            return (ret);
        }
    }

    public class MRADSRawCSV : MRADSRawData, ImportSource
    {
        FileStream fs;

        DateTime _start, _end;

        public override string Filename => fs.Name;

        public override DateTime Start => _start;
        public override DateTime End => _end;

        public override object Position
        {
            get
            {
                return (fs.Position);
            }

            set
            {
                fs.Position = (long)value;
            }
        }

        public MRADSRawCSV()
        {
        }

        public void OpenFile(string filename)
        {
            fs = new FileStream(filename, FileMode.Open);

            SetStartAndEnd();
        }

        void SetStartAndEnd()
        {
            CANMessage msg;

            msg = ReadNext();
            _start = msg.Time;

            GotoEnd();

            msg = ReadNext();
            _end = msg.Time;

            GotoStart();
        }

        override public void GotoStart()
        {
            fs.Seek(0, SeekOrigin.Begin);
        }

        override public void GotoEnd()
        {
            fs.Seek(0, SeekOrigin.End);

            ReadPrev();
        }

        override public void GotoTime(DateTime time)
        {
            long pos;
            CANMessage msg;
            bool moveforward = true, setdirection = false;

            long guess = (long)((time - Start).TotalSeconds / (End - Start).TotalSeconds * fs.Length);

            fs.Seek(guess, SeekOrigin.Begin);
            RewindToLineEnd();

            do
            {
                pos = fs.Position;

                if (moveforward)
                    msg = ReadNext();
                else
                    msg = ReadPrev();

                if (msg == null)
                    throw new Exception("Time not within the boundaries of the source");

                if (!setdirection)
                {
                    moveforward = msg.Time < time;
                    setdirection = true;
                }

                if (msg.Time == time)
                    break;
                else if (msg.Time < time && !moveforward)
                    break;
                else if (msg.Time > time && moveforward)
                    break;

            } while (msg != null);

            fs.Seek(pos, SeekOrigin.Begin);
        }

        override public CANMessage ReadNext()
        {
            CANMessage ret = null;

            while (fs.Position < fs.Length && ret == null)
                ret = CANMessage.FromCSV(ReadLine(), _start);

            return ret;
        }

        const int chunksize = 256;

        string ReadLine()
        {
            byte[] buf = new byte[chunksize];

            var len = fs.Read(buf, 0, chunksize);

            var t = Encoding.ASCII.GetString(buf);
            var idx = t.IndexOf('\n');

            fs.Seek(-(len - (idx + 1)), SeekOrigin.Current);

            return (t.Substring(0, idx).Trim());
        }

        string RewindToLineEnd()
        {
            byte[] buf = new byte[chunksize];
            string str;
            int i, rlen;

            if (fs.Position == 0)
                return (string.Empty);
            else if (fs.Position < chunksize)
            {
                rlen = (int)fs.Position;
                fs.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                rlen = chunksize;
                fs.Seek(-chunksize, SeekOrigin.Current);
            }

            var len = fs.Read(buf, 0, rlen);

            for (i = len - 1; i >= 0; i--)
            {
                if (buf[i] == '\n' && i != len - 1)
                    break;
            }
            
            i++;

            str = Encoding.ASCII.GetString(buf, i, len - i);

            fs.Seek(-str.Length, SeekOrigin.Current);

            return (str);
        }

        public override CANMessage ReadPrev()
        {
            if (fs.Position == 0)
                return (null);

            return (CANMessage.FromCSV(RewindToLineEnd()));
        }

        public override void Close()
        {
            if (fs != null)
                fs.Close();
        }
    }

    public interface ICANMessage
    {
        public double TimeStamp { get; }
        public DateTime Time { get; }

        public int Channel { get; }

        public byte Priority { get; }
        public ushort PGN { get; }
        public byte Source { get; }

        public byte[] Data { get; }
    }

    public class CANMessage : ICANMessage
    {
        public double TimeStamp { get; private set; }
        public DateTime Time { get; private set; }

        public int Channel { get; private set; }

        public byte Priority { get; private set; }
        public ushort PGN { get; private set; }
        public byte Source { get; private set; }

        public byte[] Data { get; private set; }

        CANMessage()
        {
        }

        public CANMessage(double timestamp, DateTime utctime, int channel, uint id, byte[] data)
        {
            TimeStamp = timestamp;
            Time = utctime;

            Channel = channel;

            Priority = (byte)(id >> 24);
            PGN = (ushort)(id >> 8);
            Source = (byte)id;

            Data = new byte[8];
            Array.Copy(data, Data, Data.Length);
        }

        static CANMessage FromMRADS1CSV(string[] sp)
        {

            int i;
            uint id;
            CANMessage ret = new CANMessage();

            ret.Channel = int.Parse(sp[0]);
            ret.Time = DateTime.Parse(sp[1]);

            id = uint.Parse(sp[2], System.Globalization.NumberStyles.HexNumber);
            ret.Priority = (byte)(id >> 24);
            ret.PGN = (ushort)(id >> 8);
            ret.Source = (byte)id;

            ret.Data = new byte[8];

            var d = sp[3].Split(' ');

            for (i = 0; i < 8; i++)
                ret.Data[i] = byte.Parse(d[i], System.Globalization.NumberStyles.HexNumber);

            return (ret);
        }

        public static CANMessage FromCSV(string line, DateTime UTCbase = default)
        {
            CANMessage ret;
            string[] sp = line.Split(',');
            int i, len;
            uint id;

            if (sp.Length == 4)
                return (FromMRADS1CSV(sp));
            else if (sp.Length < 15)
                throw new Exception("Invalid CAN message");

            ret = new CANMessage();

            ret.TimeStamp = double.Parse(sp[0]);

            if (UTCbase != default)
                ret.Time = UTCbase.AddSeconds(ret.TimeStamp);
            else
                ret.Time = DateTime.Parse(sp[14]);

            ret.Channel = int.Parse(sp[1]);

            if (!uint.TryParse(sp[2], NumberStyles.HexNumber, CultureInfo.CurrentCulture, out id))
                return null;

            ret.Priority = (byte)(id >> 24);
            ret.PGN = (ushort)(id >> 8);
            ret.Source = (byte)id;

            ret.Data = new byte[8];

            if (!int.TryParse(sp[4], out len))
                return null;

            for (i = 0; i < len; i++)
            {
                if (!byte.TryParse(sp[5 + i], NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ret.Data[i]))
                    ret.Data[i] = 0;
            }

            return (ret);
        }

        public string ToCSV()
        {
            uint id = (uint)((Priority << 24) | (PGN << 8) | Source);

            return ($"{Channel},{Time.ToString("yyyy-MM-dd HH:mm:ss.fff")},{id:x8},{Data.Select(a => a.ToString("x2")).Aggregate((a, b) => a + " " + b)}");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRADS2
{
    /// <summary>
    /// An MRADSDataSource implementation using the proprietary mradsdb file format:
    /// 
    /// Header:
    /// 5 byte ID ("MRADS")
    /// 2 byte version
    /// 8 byte section table location
    /// 8 byte section table length
    /// 
    /// Section Table Entry:
    /// 1 byte section ID
    /// 8 byte section location
    /// 8 byte section length
    /// 
    /// Two section types are currently defined: CAN messages and events.
    /// 
    /// This class also implements the ImportSource interface which allows data to be
    /// imported from it (for extracting a subset for instance).
    /// 
    /// A buffer of messages (defaults to 256) is held in memory to prevent constantly needing
    /// to access the disk. This buffer is updated as the Position changes.
    /// </summary>
    class MRADSDBSource : MRADSDataSource, ImportSource
    {
        const int MaxBufferCount = 256;
        int BufferCount;

        FileStream fs;
        int bufferidx = -2;
        byte[] bytebuffer;
        long lastreadpos;
        // A list of the buffered messages
        List<MRADSDataImporter.MsgEntry> msgs = new List<MRADSDataImporter.MsgEntry>();

        public override string Filename => fs.Name;

        public override List<MRADSEvent> Events { get; } = new List<MRADSEvent>();

        public override DateTime Start => start;
        public override DateTime End => end;

        DateTime start, end;

        MRADSDataImporter.Header header;
        MRADSDataImporter.SectionTable sectiontable;
        MRADSDataImporter.SectionTable.SectionEntry msgsection;

        /// <summary>
        /// In this context, Position points to the actual location in the file of the current message
        /// </summary>
        protected override object Position
        {
            get
            {
                if (bufferidx == -2)
                    return (fs.Position - MsgIndex);
                else
                    return (fs.Position - MsgIndex - MRADSDataImporter.MsgEntry.Size * (BufferCount - bufferidx));
            }

            set
            {
                long req = (long)value + MsgIndex;

                if ((long)value > MsgEnd || req > MsgEnd)
                    req = MsgEnd;

                fs.Position = req;

                bufferidx = -2;
            }
        }

        long MsgIndex => (long)msgsection.Location;
        long MsgEnd => (long)(msgsection.Location + msgsection.Length);

        public void OpenFile(string filename)
        {
            fs = new FileStream(filename, FileMode.Open, FileAccess.Read);

            header = MRADSDataImporter.Header.Deserialize(fs);
            sectiontable = MRADSDataImporter.SectionTable.Deserialize(fs, header);

            msgsection = sectiontable[MRADSDataImporter.SectionTable.SectionIDs.Messages];

            // Resize the buffer down if there are only a few messages
            if (msgsection.Length < (ulong)(MaxBufferCount * MRADSDataImporter.MsgEntry.Size))
            {
                bytebuffer = new byte[msgsection.Length];
                BufferCount = (int)msgsection.Length / MRADSDataImporter.MsgEntry.Size;
            }
            // Otherwise size for the maximum amount of buffered messages
            else
            {
                bytebuffer = new byte[MRADSDataImporter.MsgEntry.Size * MaxBufferCount];
                BufferCount = MaxBufferCount;
            }

            Position = 0L;

            // Get the start time
            var firstmsg = NextMsg();
            start = firstmsg.Time;

            Position = long.MaxValue;

            // Get the end time
            var lastmsg = PrevMsg();
            end = lastmsg.Time;

            ReadEvents();

            Position = 0L;
        }

        void ReadEvents()
        {
            var eventsection = sectiontable[MRADSDataImporter.SectionTable.SectionIDs.Events];

            byte[] buf = new byte[eventsection.Length];
            int index = 0;

            fs.Position = (long)eventsection.Location;

            fs.Read(buf, 0, buf.Length);

            while (index < buf.Length)
                Events.Add(MRADSDataImporter.BinaryDeserialize<MRADSEvent>(buf, ref index));
        }

        protected override void CloseSource()
        {
            if (fs == null)
                return;

            fs.Close();
            fs = null;
        }

        protected override void GotoTime(DateTime time)
        {
            long pos;

            // Aproximately calculate where the messages of interest start
            pos = (long)((time - Start).Ticks / (double)(End - Start).Ticks * (fs.Length - MsgIndex) / MRADSDataImporter.MsgEntry.Size) * MRADSDataImporter.MsgEntry.Size;

            Position = pos;

            var msg = NextMsg();

            // Iterate forwards or backwards until the time is reached
            if (msg.Time > time)
            {
                do
                {
                    msg = PrevMsg();
                } while (msg != null && msg.Time > time);
            }
            else if (msg.Time < time)
            {
                do
                {
                    msg = NextMsg();
                } while (msg != null && msg.Time < time);
                
                PrevMsg();
            }
        }

        /// <summary>
        /// Update the buffered messages
        /// </summary>
        /// <param name="forward">Buffer in the forward direction (true) or reverse (false)</param>
        /// <returns>Returns true if the buffer was updated or false if the ends of the data has been reached</returns>
        bool ReadIntoBuffer(bool forward = true)
        {
            int i;
            long pos;
            DateTime ptime;

            if (bufferidx != -2 && msgs.Count > 0)
                ptime = msgs[bufferidx].Time;
            else if (forward)
                ptime = DateTime.MinValue;
            else
                ptime = DateTime.MaxValue;

            if (!forward)
            {
                // If we're already at the start of the data return false
                if (bufferidx != -2 && msgs.Count > 0 && msgs[0].Time == Start)
                    return (false);

                // Adjust the reading position backwards
                pos = fs.Position - bytebuffer.Length;

                // Adjust backwards again if we have previously read data to fill the buffer to compensate
                // for the fact that the read position is moved forward after a read
                if (bufferidx != -2)
                    pos -= bytebuffer.Length;

                if (pos < MsgIndex)
                {
                    // Can't move a full buffer size backwards
                    pos = MsgIndex;
                    bufferidx = (int)(MsgIndex - pos) / MRADSDataImporter.MsgEntry.Size;
                }
                else
                    bufferidx = -2;
            }
            else
            {
                // If we're already at the end of the data return false
                if (bufferidx != -2 && msgs.Count > 0 && msgs[msgs.Count - 1].Time == End)
                    return (false);

                if ((fs.Position + bytebuffer.Length) > MsgEnd)
                {
                    // Can't move a full buffer size forwards
                    pos = MsgEnd - bytebuffer.Length;
                    bufferidx = (int)((fs.Position + bytebuffer.Length) - MsgEnd) / MRADSDataImporter.MsgEntry.Size;
                    if (bufferidx >= (BufferCount - 1))
                        bufferidx = BufferCount - 2;
                }
                else
                {
                    pos = fs.Position;
                    bufferidx = -2;
                }
            }

            fs.Position = pos;

            lastreadpos = fs.Position;

            // Read the new data into the buffer
            var len = fs.Read(bytebuffer, 0, bytebuffer.Length);

            msgs.Clear();

            for (i = 0; i < len / MRADSDataImporter.MsgEntry.Size; i++)
            {
                var msg = MRADSDataImporter.MsgEntry.Deserialize(bytebuffer, MRADSDataImporter.MsgEntry.Size * i);

                // Update the bufferidx to point to the next message in the data by lining up the message timestamps
                if (bufferidx == -2 && forward && msg.Time >= ptime)
                    bufferidx = i - 1;
                else if (!forward && msg.Time <= ptime)
                    bufferidx = i + 1;

                msgs.Add(msg);
            }

            return (true);
        }

        protected override ICANMessage NextMsg()
        {
            if (bufferidx == -2 || bufferidx == (msgs.Count - 1))
            {
                if (!ReadIntoBuffer())
                    return (null);
            }

            return (msgs[++bufferidx]);
        }

        protected override ICANMessage PrevMsg()
        {
            if (bufferidx == -2 || bufferidx == 0)
            {
                if (!ReadIntoBuffer(false))
                    return (null);
            }

            return (msgs[--bufferidx]);
        }

        public void GotoStart()
        {
            Position = 0L;
        }

        public CANMessage ReadNext()
        {
            var next = NextMsg();

            if (next == null)
                return (null);

            return (new CANMessage(0, next.Time, next.Channel, (uint)((next.Priority << 24) | (next.PGN << 8) | next.Source), next.Data));
        }

        public void Invalidate()
        {
            bufferidx = -2;
        }
    }
}

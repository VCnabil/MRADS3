using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kvaser.KvmLib;

namespace MRADS2
{
    /// <summary>
    /// Import source for the native Memorator file format
    /// </summary>
    class MRADSKMFSource : ImportSource
    {
        public string Filename => filename;

        public DateTime Start { get; private set; }

        public DateTime End { get; private set; }

        /// <summary>
        /// Memorator data is stored in a series of log files
        /// </summary>
        class LogFile
        {
            public int Index { get; }
            public long Events { get; }
            public DateTime Start { get; }
            public DateTime End { get; }

            public LogFile(int index, long events, long start, long end)
            {
                Index = index;
                Events = events;

                Start = DateTimeOffset.FromUnixTimeSeconds(start).UtcDateTime;
                End = DateTimeOffset.FromUnixTimeSeconds(end).UtcDateTime;
            }
        }

        List<LogFile> logs;
        Kvmlib.Handle handle;
        int mountedlogidx = -1;
        DateTime tsbase;
        bool offsetset = false;
        long tsoffsetnano;
        string filename;

        public void Close()
        {
            if (handle.handle == IntPtr.Zero)
                return;

            if (mountedlogidx != -1)
            {
                Kvmlib.LogFileDismount(handle);
                mountedlogidx = -1;
            }

            Kvmlib.Close(handle);
        }

        public void OpenFile(string filename)
        {
            Kvmlib.Status status;

            Kvmlib.Initialize();

            handle = Kvmlib.KmfOpen(filename, out status, Kvmlib.DeviceType.kvmDEVICE_MHYDRA_EXT);

            if (status != Kvmlib.Status.OK)
            {
                handle = default;

                throw new Exception("Error opening KMF file");
            }

            this.filename = filename;

            logs = ReadLogs();

            Start = logs.Min(a => a.Start);
            End = logs.Max(a => a.End);

            tsbase = Start;
            tsoffsetnano = 0;
            mountedlogidx = -1;
        }

        /// <summary>
        /// Read all of the log file information
        /// </summary>
        /// <returns></returns>
        List<LogFile> ReadLogs()
        {
            int i, logcount;
            List<LogFile> logs = new List<LogFile>();

            Kvmlib.LogFileGetCount(handle, out logcount);

            for (i = 0; i < logcount; i++)
            {
                int t1, t2;
                long events;

                Kvmlib.LogFileMountEx(handle, i, out events);

                Kvmlib.LogFileGetStartTime(handle, out t1);
                Kvmlib.LogFileGetEndTime(handle, out t2);

                logs.Add(new LogFile(i, events, t1, t2));

                Kvmlib.LogFileDismount(handle);
            }

            return (logs);
        }

        public CANMessage ReadNext()
        {
            Kvmlib.Log ev;
            Kvmlib.Status status;
            DateTime tsnow;
            long tsnano = 0;
            CANMessage ret = null;

            if (mountedlogidx == -1)
                MountLog(0);

            do
            {
                status = Kvmlib.LogFileReadEvent(handle, out ev);
                if (status == Kvmlib.Status.EOF || status == Kvmlib.Status.ERR_NOLOGMSG)
                {
                    // If we hit the end of the log, move to the next log if possible
                    Kvmlib.LogFileDismount(handle);

                    if ((mountedlogidx + 1) == logs.Count)
                    {
                        mountedlogidx = -1;

                        return (null);
                    }

                    MountLog(mountedlogidx + 1);

                    continue;
                }
                else if (status != Kvmlib.Status.OK)
                    return (null);

                switch (ev)
                {
                    case Kvmlib.LogMsg m:
                        tsnano = m.timeStamp;
                        break;
                    case Kvmlib.LogTrigger t:
                        tsnano = t.timeStamp;
                        break;
                    case Kvmlib.LogRtcClock t:
                        tsnano = t.timeStamp;
                        break;
                    case Kvmlib.LogVersion v:
                        continue;
                }

                // Capture the first timestamp to use as the reference point
                if (!offsetset)
                {
                    tsoffsetnano = tsnano;

                    offsetset = true;
                }

                tsnow = tsbase.AddTicks((tsnano - tsoffsetnano) / 100);

                if (ev is Kvmlib.LogMsg)
                {
                    var msg = (Kvmlib.LogMsg)ev;

                    ret = new CANMessage(msg.timeStamp / 1E9, tsnow, msg.channel + 1, (uint)msg.id, msg.data);
                }
            } while (ret == null);

            return (ret);
        }

        void MountLog(int idx)
        {
            Kvmlib.LogFileMountEx(handle, logs[idx].Index, out _);

            tsbase = logs[idx].Start;
            offsetset = false;

            mountedlogidx = idx;
        }

        public void GotoStart()
        {
            if (mountedlogidx != -1)
            {
                Kvmlib.LogFileDismount(handle);

                mountedlogidx = -1;
            }
        }
    }
}

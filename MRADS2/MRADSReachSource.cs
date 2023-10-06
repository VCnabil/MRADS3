using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MRADS2
{
    class MRADSReachSource : ImportSource
    {
        public string Filename { get; private set; }

        public DateTime Start { get; private set; }

        public DateTime End { get; private set; }

        Dictionary<int, FileStream> channels = new Dictionary<int, FileStream>();
        Dictionary<int, CANMessage> channelmsgs = new Dictionary<int, CANMessage>();

        public void Close()
        {
            foreach (var v in channels.Values)
                v.Close();

            channels.Clear();
        }

        public void GotoStart()
        {
            foreach (var v in channels.Values)
                v.Position = 0;
        }

        public void OpenFile(string filename)
        {
            Filename = filename;

            var xml = new XmlDocument();
            xml.Load(filename);

            Start = DateTime.UnixEpoch.AddSeconds(ulong.Parse(xml["Recording"]["StartTime"].InnerText));
            End = DateTime.UnixEpoch.AddSeconds(ulong.Parse(xml["Recording"]["StopTime"].InnerText));

            foreach (XmlElement child in xml["Recording"].ChildNodes)
            {
                if (child.Name.StartsWith("File"))
                {
                    string fn = child.InnerText;
                    int channel = int.Parse(Path.GetExtension(fn).Substring(4));

                    channels[channel] = new FileStream($"{Path.GetDirectoryName(filename)}\\CAN{channel}\\{fn}", FileMode.Open, FileAccess.Read);
                    channelmsgs[channel] = null;
                }
            }
        }

        CANMessage ReadMessage(int channel)
        {
            var fs = channels[channel];
            byte[] buffer = new byte[24];
            uint majtime, mintime;
            uint canid, canframe;
            byte[] data = new byte[8];
            int idx = 0;
            DateTime dt;

            if (fs.Position == fs.Length)
                return (null);

            fs.Read(buffer, 0, buffer.Length);

            majtime = BitConverter.ToUInt32(buffer, idx);
            idx += 4;

            mintime = BitConverter.ToUInt32(buffer, idx);
            idx += 4;

            canid = BitConverter.ToUInt32(buffer, idx);
            idx += 4;

            canframe = BitConverter.ToUInt32(buffer, idx);
            idx += 4;

            Array.Copy(buffer, idx, data, 0, data.Length);

            dt = DateTime.UnixEpoch.AddSeconds(majtime + mintime / 1E9);

            return (new CANMessage(0, dt, channel, canid, data));
        }

        public CANMessage ReadNext()
        {
            CANMessage ret = null;

            foreach (var kvp in channelmsgs)
            {
                if (kvp.Value == null)
                    channelmsgs[kvp.Key] = ReadMessage(kvp.Key);

                if (channelmsgs[kvp.Key] != null && (ret == null || channelmsgs[kvp.Key].Time < ret.Time))
                    ret = channelmsgs[kvp.Key];
            }

            if (ret != null)
                channelmsgs[ret.Channel] = null;

            return (ret);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRADS2
{
    public class MRADSEvent
    {
        public enum EventCategory : byte { Fault, Warning, Status, OnlineOffline }
        static readonly string[] EventNames = new string[] { "Fault", "Warning", "Status", "Online/Offline" };
        static readonly Color[] EventColors = new Color[] { Color.Red, Color.Orange, Color.Black, Color.Black };

        public class VariableID
        {
            public string Provider;
            public string Variable;

            public VariableID(string provider, string variable)
            {
                Provider = provider;
                Variable = variable;
            }

            public override string ToString()
            {
                return ($"{Provider}.{Variable}");
            }
        }

        public long TimeTicks
        {
            get => Time.Ticks;
            set => Time = new DateTime(value);
        }

        [MRADSDataImporter.DontSerialize]
        public DateTime Time { get; set; }
        public EventCategory Category { get; set; }
        public string Description { get; set; }

        public string[] VariableStrings
        {
            get => Variables.Select(a => a.ToString()).ToArray();
            set
            {
                Variables = new VariableID[value.Length];
                int i = 0;

                foreach (var s in value)
                {
                    var sp = s.Split('.');

                    Variables[i++] = new VariableID(sp[0], sp[1]);
                }
            }
        }

        [MRADSDataImporter.DontSerialize]
        public VariableID[] Variables { get; private set; }

        [MRADSDataImporter.DontSerialize]
        public Color EventColor => EventColors[(int)Category];

        public MRADSEvent(DateTime time, EventCategory category, string description, params VariableID[] variables)
        {
            Time = time;
            Category = category;
            Description = description;
            Variables = variables;
        }

        public MRADSEvent()
        {

        }

        public override string ToString()
        {
            string catname;
            int catval;

            catval = (int)Category;
            if (catval < EventNames.Length)
                catname = EventNames[catval];
            else
                catname = "Unknown";

            return ($"{Time.ToString("yyyy-MM-dd HH:mm:ss.fff")}, {catname}, {Description}");
        }
    }

    /// <summary>
    /// An abstract class used to implement event handlers that can monitor and generate events
    /// </summary>
    public abstract class MRADSEventHandler
    {
        protected MRADSShip ship;

        public virtual IEnumerable<MRADSEvent> EventCheck(MRADSShipState state, MRADSDataImporter.MsgEntry msg, MRADSDataChangeSet changes)
        {
            return (null);
        }

        public virtual IEnumerable<MRADSEvent> FirstMessage(MRADSShipState state, MRADSDataImporter.MsgEntry msg)
        {
            return (null);
        }

        public virtual IEnumerable<MRADSEvent> LastMessage(MRADSShipState state, MRADSDataImporter.MsgEntry msg)
        {
            return (null);
        }

        public MRADSEventHandler(MRADSShip ship)
        {
            this.ship = ship;
        }
    }
}

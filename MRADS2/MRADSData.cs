using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MRADS2
{
    /// <summary>
    /// A class representing the MRADS data and the ship it's relevant to
    /// </summary>
    public class MRADSData
    {
        public MRADSShip Ship { get; private set; }

        public MRADSDataSource Source { get; }

        public MRADSData(MRADSDataSource source)
        {
            this.Source = source;
        }

        public void SetShip(MRADSShip ship)
        {
            Ship = ship;

            Source.Init();
        }
    }

    /// <summary>
    /// A class that abstracts the access to the MRADS data.
    /// This class contains a state object detailing the current position within the data. When
    /// the position is changed, the current state of the ship variables are updated either synchronously or asynchronously
    /// in a separate thread from the UI. A callback (StateUpdateCallback) is available which will be called
    /// once the update has completed. Data for the given time window (defaults to 60 seconds) is buffered in a list of
    /// ship variable changes (List<MRADSDataChangeSet> changes) at each time point. Iterating this list and applying
    /// the variable changes evolves the ship variable values from the start to the end of the window.
    /// </summary>
    public abstract class MRADSDataSource
    {
        public const int BufferedSeconds = 60;
        public const int MaxSearch = BufferedSeconds;

        public DateTime BufferStart => basestate.Time;
        public DateTime BufferEnd => endstate.Time;

        protected MRADSShip Ship;

        abstract public string Filename { get; }

        // The start time of the data
        abstract public DateTime Start { get; }
        // The end time of the data
        abstract public DateTime End { get; }

        abstract public List<MRADSEvent> Events { get; }

        abstract protected void GotoTime(DateTime time);
        // Retrieve the next CAN message from the current position
        abstract protected ICANMessage NextMsg();
        // Retrieve the previous CAN message from the current position
        abstract protected ICANMessage PrevMsg();

        // A handle to an object specifying the current position within the data
        abstract protected object Position { get; set; }

        abstract protected void CloseSource();

        public Action<bool> StateUpdateCallback = null;

        List<MRADSDataChangeSet> changes = new List<MRADSDataChangeSet>();
        // The basestate is the state of the ship variables at the beginning of the time window
        // The endstate is the state of the ship variables at the end of the time window
        MRADSShipState basestate = null, endstate = null;
        object basestatepos = null, endstatepos = null;

        CancellationTokenSource ct;
        Thread datathread;
        SynchronizationContext sctx;
        AutoResetEvent datathreadstart, datathreadcbdone;

        public MRADSDataSource()
        {
            sctx = SynchronizationContext.Current;

            ct = new CancellationTokenSource();

            datathreadstart = new AutoResetEvent(false);
            datathreadcbdone = new AutoResetEvent(false);

            datathread = new Thread(DataThread);
            datathread.Start();
        }

        public void SetShip(MRADSShip ship)
        {
            Ship = ship;
        }

        public void Init()
        {
            SetTimeSync(Start);
        }

        public void Close()
        {
            ct.Cancel();

            datathread.Join();

            CloseSource();

            changes.Clear();
            Events.Clear();
        }

        /// <summary>
        /// A request to change the current position to a new time
        /// </summary>
        class DataThreadReq
        {
            // Setting AdjustBuffer to true will cause the ship variable state at the beginning of the time
            // window to be recomputed. This should only be set to true when the time window has been altered.
            public bool AdjustBuffer;
            public DateTime Time;
        }

        DataThreadReq req = new DataThreadReq();

        void DataThread()
        {
            bool adjustbuffer;
            DateTime time;
            WaitHandle[] waithandles = new WaitHandle[] { ct.Token.WaitHandle, datathreadstart };
            WaitHandle[] cbwaithandles = new WaitHandle[] { ct.Token.WaitHandle, datathreadcbdone };

            while (!ct.IsCancellationRequested)
            {
                // Wait for any requests
                if (WaitHandle.WaitAny(waithandles) == 0)
                    break;
                
                lock (req)
                {
                    adjustbuffer = req.AdjustBuffer;
                    time = req.Time;
                }

                if (adjustbuffer)
                {
                    lock (changes)
                        UpdateBuffer(time);
                }

                var state = GetStateAtTime(time);

                // Call the callback on the UI thread
                sctx.Post(DataThreadCallback, (adjustbuffer, state));

                // Wait for the callback to complete
                if (WaitHandle.WaitAny(cbwaithandles) == 0)
                    break;
            }
        }

        void DataThreadCallback(object o)
        {
            (bool adjustbuffer, MRADSShipState state) result = ((bool, MRADSShipState))o;

            Ship.State.SetState(result.state);

            StateUpdateCallback?.Invoke(result.adjustbuffer);

            datathreadcbdone.Set();
        }

        public void SetTime(DateTime time, bool adjustbuffer = true)
        {
            lock (req)
            {
                req.AdjustBuffer = adjustbuffer;
                req.Time = time;
            }

            datathreadstart.Set();
        }

        public void SetTimeSync(DateTime time, bool adjustbuffer = true)
        {
            if (adjustbuffer)
                UpdateBuffer(time);

            Ship.State.SetState(GetStateAtTime(time));
        }

        public List<MRADSDataChangeSet> GetChangesAtTime(DateTime time)
        {
            UpdateBuffer(time, time.AddSeconds(BufferedSeconds));

            return changes;
        }

        public void SetBuffer(DateTime start, DateTime end)
        {
            UpdateBuffer(start, end);
        }

        /// <summary>
        /// Update the time window centered around the given time
        /// </summary>
        /// <param name="time"></param>
        void UpdateBuffer(DateTime time)
        {
            UpdateBuffer(time.AddSeconds(-BufferedSeconds / 2), time.AddSeconds(BufferedSeconds / 2));
        }

        /// <summary>
        /// Recompute the changes based on the new window specified by the start and end times
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        void UpdateBuffer(DateTime start, DateTime end)
        {
            MRADSDataChangeSet change;
            DateTime newstart = start, newend = end;

            // If the start time is earlier than the beginning of the data, clamp it
            if (newstart < Start)
            {
                newstart = Start;
                newend = Start.AddSeconds(BufferedSeconds);
            }
            // If the end time is later than the end of the data, clamp it
            else if (newend > End)
            {
                newend = End;
                newstart = End.AddSeconds(-BufferedSeconds);
            }

            // If the basestate already exists and the start time is within the existing window
            if (basestate != null && newstart >= basestate.Time && newstart <= endstate.Time)
            {
                int changeindex;

                basestate = GetStateAtTimeInternal(newstart, out changeindex);
                if (changeindex >= 0)
                    basestatepos = changes[changeindex].Position;

                // Remove all changes that fall before the start of the new window
                changes.RemoveRange(0, changeindex + 1);

                Position = endstatepos;

                NextChange();

                // Iterate through the data until the end time is reached
                while (true)
                {
                    change = NextChange();

                    if (change == null || change.Time > newend)
                        break;

                    endstatepos = change.Position;
                    endstate.UpdateState(change);

                    changes.Add(change);
                }

                endstate.ForceTime(newend);
                endstate.ExpireVariables();
            }
            else
            {
                bool addtofront = (basestate != null && newstart < basestate.Time && newend >= basestate.Time);

                // If the new start time is before the current window start time (and the new window overlaps with the old one)
                if (addtofront)
                {
                    int changeindex;

                    endstate = GetStateAtTimeInternal(newend, out changeindex);
                    if (changeindex >= 0)
                        endstatepos = changes[changeindex].Position;

                    // Remove data at the end that's now outside of the window
                    changes.RemoveRange(changeindex + 1, changes.Count - (changeindex + 1));
                }
                else
                    changes.Clear();

                GotoTime(newstart);

                basestate = new MRADSShipState(Ship);
                basestatepos = null;

                var startpos = Position;

                bool first = true;

                // Iterate backwards until all ship variable values have been set
                do
                {
                    if (first)
                    {
                        // Start by getting the first change at the specified position
                        change = NextChange();
                        first = false;

                        if (change == null)
                            continue;
                    }
                    else
                        change = PrevChange();

                    if (change == null)
                        break;

                    // If we still haven't found values for all of the variables but the delta between the start
                    // of the window and the current message is greater than the CAN timeout time then give up
                    if ((newstart - change.Time).TotalSeconds > MRADSSettings.CANTimeout)
                        break;

                    basestatepos = change.Position;
                    basestate.UpdateState(change, false, true);
                } while (!basestate.AllVariablesSet);

                if (basestatepos == null)
                    basestatepos = 0L;

                Position = startpos;

                basestate.ForceTime(newstart);
                basestate.ExpireVariables();

                var state = basestate.Clone();
                var statepos = basestatepos;

                int addtofrontcntr = 0;

                NextChange();

                // Add changes until we reach the end of the window
                while (true)
                {
                    change = NextChange();

                    if (change == null || change.Time > newend || (addtofront && (changes.Count - addtofrontcntr) > 0 && change.Time >= changes[addtofrontcntr].Time))
                        break;

                    statepos = change.Position;
                    state.UpdateState(change);

                    if (addtofront)
                        changes.Insert(addtofrontcntr++, change);
                    else
                        changes.Add(change);
                }

                // If we're not just adding to the front of the change list, update the end state variables
                if (!addtofront)
                {
                    endstate = state;
                    endstatepos = statepos;

                    endstate.ForceTime(newend);
                    endstate.ExpireVariables();
                }
            }
        }

        /// <summary>
        /// Get the ship variable state at the specified time
        /// </summary>
        /// <param name="time"></param>
        /// <param name="changeindex">An index into the changes list corresponding to this time</param>
        /// <returns>An object representing the ship's variable state at the requested time</returns>
        MRADSShipState GetStateAtTimeInternal(DateTime time, out int changeindex)
        {
            MRADSShipState ret = basestate.Clone();
            int i = 0;

            while (i < changes.Count && changes[i].Time <= time)
                ret.UpdateState(changes[i++]);

            changeindex = i - 1;

            ret.ForceTime(time);
            ret.ExpireVariables();

            return (ret);
        }

        public MRADSShipState GetStateAtTime(DateTime time)
        {
            return (GetStateAtTimeInternal(time, out _));
        }

        /// <summary>
        /// Retrieve a list of data points associated with the specified ship variables for the current time window
        /// </summary>
        /// <param name="vardefs"></param>
        /// <returns></returns>
        public Dictionary<MRADSVariableDefinition, List<MRADSDataPoint>> GetDataAtCurrentTime(IEnumerable<MRADSVariableDefinition> vardefs)
        {
            Dictionary<MRADSVariableDefinition, List<MRADSDataPoint>> ret = new Dictionary<MRADSVariableDefinition, List<MRADSDataPoint>>();

            lock (changes)
            {
                // Add the initial values from the basestate
                foreach (var v in vardefs)
                {
                    ret[v] = new List<MRADSDataPoint>();

                    var val = basestate.GetValue(v.Decoder.Provider, v.Name);

                    if (val.Set)
                        ret[v].Add(new MRADSDataPoint(basestate.Time, val));
                }

                // Add all of the changes
                foreach (var c in changes)
                {
                    foreach (var cv in c.Changes)
                    {
                        if (ret.ContainsKey(cv.VariableDefinition))
                            ret[cv.VariableDefinition].Add(new MRADSDataPoint(c.Time, cv));
                    }
                }

                // Add the end state values
                foreach (var v in vardefs)
                {
                    var val = endstate.GetValue(v.Decoder.Provider, v.Name);

                    if (val.Set)
                        ret[v].Add(new MRADSDataPoint(endstate.Time, val));
                    else if (ret[v].Count == 1)
                        ret[v].Add(new MRADSDataPoint(ret[v][0].Time, ret[v][0].Value));
                }
            }

            return (ret);
        }

        /// <summary>
        /// Retrieve the next set of changes corresponding to the specified ship variables
        /// </summary>
        /// <param name="next">Move forward (true) or reverse (false) for changes</param>
        /// <param name="vars">The list of ship variables of interest or null for all</param>
        /// <returns></returns>
        MRADSDataChangeSet GetChange(bool next, HashSet<MRADSVariableDefinition> vars)
        {
            List<MRADSVariableValue> changes = new List<MRADSVariableValue>();
            ICANMessage msg;
            MRADSDataProvider provider = null;
            PGNDecoder decoder = null;
            DateTime curtime = default;
            object position = null;

            while (changes.Count == 0)
            {
                if (next)
                {
                    msg = NextMsg();
                    position = Position;
                }
                else
                {
                    msg = PrevMsg();
                    position = Position;
                }

                if (msg == null)
                    return (null);

                // Decode the CAN message, mapping it to ship variables
                var values = Ship.Config.Decode(msg.Source, msg.Channel, msg.PGN, msg.Data, out provider, out decoder);
                if (values == null)
                    continue;

                curtime = msg.Time;

                foreach (var v in values)
                {
                    // If the CAN message contains variables specified in the vars list, add it to the changes
                    if (vars == null || vars.Contains(v.VariableDefinition))
                        changes.Add(v);
                }
            }

            return (new MRADSDataChangeSet(provider, decoder, curtime, position, changes));
        }

        protected MRADSDataChangeSet NextChange(HashSet<MRADSVariableDefinition> vars = null)
        {
            return (GetChange(true, vars));
        }

        protected MRADSDataChangeSet PrevChange(HashSet<MRADSVariableDefinition> vars = null)
        {
            return (GetChange(false, vars));
        }
    }

    public class MRADSDataChangeSet
    {
        // The CAN device that sent the message
        public MRADSDataProvider Provider { get; }
        // The decoder for this message
        public PGNDecoder Decoder { get; }
        // The ship variable values
        public List<MRADSVariableValue> Changes { get; }
        // The time that the CAN message arrived
        public DateTime Time { get; }
        // The position in the data of this CAN message
        public object Position { get; }

        public MRADSDataChangeSet(MRADSDataProvider provider, PGNDecoder decoder, DateTime time, object position, List<MRADSVariableValue> changes)
        {
            Provider = provider;
            Decoder = decoder;
            Changes = changes;
            Time = time;
            Position = position;
        }
    }

    public class MRADSDataPoint
    {
        public DateTime Time { get; }
        public MRADSVariableValue Value { get; }

        public MRADSDataPoint(DateTime time, MRADSVariableValue value)
        {
            Time = time;
            Value = value;
        }
    }

    /// <summary>
    /// An implementation of the MRADSDataSource abstract class for raw data input files
    /// </summary>
    public class MRADSRawDataSource : MRADSDataSource
    {
        // An object providing access to the raw data
        MRADSRawData rawdata;

        public override string Filename => rawdata.Filename;

        public override DateTime Start => rawdata.Start;
        public override DateTime End => rawdata.End;

        protected override object Position
        {
            get
            {
                return (rawdata.Position);
            }

            set
            {
                rawdata.Position = value;
            }
        }

        // Raw data input files don't categorize events so just create an empty list
        public override List<MRADSEvent> Events { get; } = new List<MRADSEvent>();

        public MRADSRawDataSource(MRADSRawData rawdata)
        {
            this.rawdata = rawdata;
        }

        protected override void GotoTime(DateTime time)
        {
            rawdata.GotoTime(time);
        }

        protected override ICANMessage NextMsg()
        {
            return rawdata.ReadNext();
        }

        protected override ICANMessage PrevMsg()
        {
            return rawdata.ReadPrev();
        }

        protected override void CloseSource()
        {
            rawdata.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRADS2.Ships
{
    /// <summary>
    /// Abstract class used to implement fault event checks
    /// </summary>
    abstract class StandardFaultEvent : MRADSEventHandler
    {
        class Fault
        {
            public MRADSDataProvider Provider => Variable.Decoder.Provider;
            public MRADSVariableDefinition Variable { get; }
            public string Description { get; }

            public Fault(MRADSDataProvider provider, string variable, string description)
            {
                Variable = provider.GetVariableDefinition(variable);
                Description = description;
            }
        }

        Dictionary<MRADSVariableDefinition, (Fault info, bool prevval)> monitoredfaults = new Dictionary<MRADSVariableDefinition, (Fault info, bool prevval)>();

        public StandardFaultEvent(MRADSShip ship) : base(ship)
        {
        }

        public override List<MRADSEvent> EventCheck(MRADSShipState state, MRADSDataImporter.MsgEntry msg, MRADSDataChangeSet changes)
        {
            List<MRADSEvent> ret = new List<MRADSEvent>();
            (Fault info, bool prevval) finfo;

            foreach (var c in changes.Changes)
            {
                if (!monitoredfaults.TryGetValue(c.VariableDefinition, out finfo))
                    continue;

                var val = c.GetValue<bool>();

                // Check if the value has changed
                if (val != finfo.prevval)
                {
                    // If it has and the value is true then a new fault has occurred
                    if (val)
                        ret.Add(new MRADSEvent(state.Time, MRADSEvent.EventCategory.Fault, finfo.info.Description, new MRADSEvent.VariableID(finfo.info.Provider.Name, c.VariableDefinition.Name)));

                    monitoredfaults[c.VariableDefinition] = (finfo.info, val);
                }
            }

            return (ret);
        }

        protected void AddCUFault(string variable, string description)
        {
            foreach (var cu in ship.ControlUnits)
                AddFault(cu.Name, variable, description);
        }

        protected void AddFault(string provider, string variable, string description)
        {
            var info = new Fault(ship.Config.GetProvider(provider), variable, provider + ": " + description);

            monitoredfaults.Add(info.Variable, (info, false));
        }
    }

    /// <summary>
    /// A standard event handler for first and last message events
    /// </summary>
    class StandardMessageEvent : MRADSEventHandler
    {
        public StandardMessageEvent(MRADSShip ship) : base(ship)
        {
        }

        public override MRADSEvent[] FirstMessage(MRADSShipState state, MRADSDataImporter.MsgEntry msg)
        {
            return (new MRADSEvent[] { new MRADSEvent(msg.Time, MRADSEvent.EventCategory.Status, "Time of first logged message") });
        }

        public override MRADSEvent[] LastMessage(MRADSShipState state, MRADSDataImporter.MsgEntry msg)
        {
            return (new MRADSEvent[] { new MRADSEvent(msg.Time, MRADSEvent.EventCategory.Status, "Time of last logged message") });
        }
    }

    /// <summary>
    /// An event handler for providers going online and offline
    /// </summary>
    class StandardOnlineOfflineEvent : MRADSEventHandler
    {
        Dictionary<MRADSDataProvider, (DateTime lastmsg, bool online)> providermsgtimes = new Dictionary<MRADSDataProvider, (DateTime lastmsg, bool online)>();

        public StandardOnlineOfflineEvent(MRADSShip ship) : base(ship)
        {
            foreach (var provider in ship.Config.GetProviders())
                providermsgtimes[provider] = (DateTime.MinValue, false);
        }

        List<MRADSEvent> CheckOffline(DateTime msgtime)
        {
            List<MRADSEvent> ret = new List<MRADSEvent>();

            foreach (var kvp in providermsgtimes)
            {
                // If a message hasn't been received from a provider in greater than CANTimeout then mark it offline
                if (kvp.Value.online && (msgtime - kvp.Value.lastmsg).TotalSeconds > MRADSSettings.CANTimeout)
                {
                    ret.Add(new MRADSEvent(kvp.Value.lastmsg, MRADSEvent.EventCategory.OnlineOffline, kvp.Key.Name + " is offline" /*, new MRADSEvent.VariableID(kvp.Key.Name, "ProviderOnline") */));

                    providermsgtimes[kvp.Key] = (kvp.Value.lastmsg, false);
                }
            }

            return (ret);
        }

        public override IEnumerable<MRADSEvent> EventCheck(MRADSShipState state, MRADSDataImporter.MsgEntry msg, MRADSDataChangeSet changes)
        {
            List<MRADSEvent> ret;

            ret = new List<MRADSEvent>();

            // If a message is received from this provider and it's currently marked as offline then add an online event
            if (!providermsgtimes[changes.Provider].online)
                ret.Add(new MRADSEvent(msg.Time, MRADSEvent.EventCategory.OnlineOffline, changes.Provider.Name + " is online" /*, new MRADSEvent.VariableID(changes.Provider.Name, "ProviderOnline") */));

            providermsgtimes[changes.Provider] = (msg.Time, true);

            ret.AddRange(CheckOffline(msg.Time));

            return (ret);
        }
    }

    /// <summary>
    /// Framework class for most event types which will involve monitoring a variable for changes
    /// </summary>
    public abstract class StandardVariableChangeEvent : MRADSEventHandler
    {
        protected class StateVal
        {
            public object State { get; set; } = null;
        }

        class StatusChangeEvent
        {
            public MRADSEvent.EventCategory Category { get; }

            Func<MRADSVariableValue, MRADSVariableValue, StateVal, string> evalfunc;

            bool waitforinitval;
            MRADSVariableValue prevval = null;

            StateVal stateval = new StateVal();

            /// <summary>
            /// A standard status change event
            /// </summary>
            /// <param name="evalfunc">The evaluation function to run to indicate if the variable change warrants an event and the event message to log</param>
            /// <param name="waitforinitval">Wait for the initial value of the variable to be known before looking for events</param>
            /// <param name="category">The event category</param>
            public StatusChangeEvent(Func<MRADSVariableValue, MRADSVariableValue, StateVal, string> evalfunc, bool waitforinitval, MRADSEvent.EventCategory category)
            {
                this.evalfunc = evalfunc;
                this.waitforinitval = waitforinitval;

                Category = category;
            }

            public string GetMessage(MRADSVariableValue val)
            {
                string ret = null;

                bool valchanged = (prevval == null && val != null) || (prevval != null && val == null) || !val.Equal(prevval);

                if (!valchanged)
                    return (null);

                if (!waitforinitval || (waitforinitval && prevval != null))
                    ret = evalfunc(val, prevval, stateval);

                prevval = val.Clone();

                return (ret);
            }
        }

        Dictionary<MRADSVariableDefinition, StatusChangeEvent> statuschanges = new Dictionary<MRADSVariableDefinition, StatusChangeEvent>();

        public StandardVariableChangeEvent(MRADSShip ship) : base(ship)
        {
        }

        public override IEnumerable<MRADSEvent> EventCheck(MRADSShipState state, MRADSDataImporter.MsgEntry msg, MRADSDataChangeSet changes)
        {
            List<MRADSEvent> ret = new List<MRADSEvent>();

            foreach (var c in changes.Changes)
            {
                StatusChangeEvent statuschange;

                if (!statuschanges.TryGetValue(c.VariableDefinition, out statuschange))
                    continue;

                var m = statuschange.GetMessage(c);
                if (m == null)
                    continue;

                ret.Add(new MRADSEvent(msg.Time, statuschange.Category, m, new MRADSEvent.VariableID(c.VariableDefinition.Decoder.Provider.Name, c.VariableDefinition.Name)));
            }

            return (ret);
        }

        protected void AddEvent(string providername, string variable, bool waitforinitval, Func<MRADSVariableValue, MRADSVariableValue, StateVal, string> evalfunc, MRADSEvent.EventCategory category = MRADSEvent.EventCategory.Status)
        {
            var provider = ship.Config.GetProvider(providername);

            statuschanges[provider.GetVariableDefinition(variable)] = new StatusChangeEvent(evalfunc, waitforinitval, category);
        }

        protected void AddCUEvent(string variable, bool waitforinitval, Func<MRADSVariableValue, MRADSVariableValue, string> evalfunc, MRADSEvent.EventCategory category = MRADSEvent.EventCategory.Status)
        {
            foreach (var cu in ship.ControlUnits)
                AddEvent(cu.Name, variable, waitforinitval, (v, p, _) =>
                {
                    var r = evalfunc(v, p);
                    if (r != null)
                        return ($"{cu.Name}: {r}");
                    else
                        return (null);
                }, category);
        }

        protected void AddBoolEvent(string providername, string variable, string falseval, string trueval, MRADSEvent.EventCategory category = MRADSEvent.EventCategory.Status)
        {
            AddEvent(providername, variable, true, (v, _, _) => v.GetValue<bool>() ? trueval : falseval, category);
        }

        protected string ThresholdFunc<T>(MRADSVariableValue v, MRADSVariableValue p, T threshold, string falseval, string trueval)
        {
            var vallt = (dynamic)v.GetValue<T>() < threshold;

            if (p == null)
                return (vallt ? falseval : trueval);

            var pvallt = (dynamic)p.GetValue<T>() < threshold;

            if (vallt && !pvallt)
                return (falseval);
            else if (!vallt && pvallt)
                return (trueval);
            else
                return (null);
        }

        /// <summary>
        /// Add an event that checks for a variable crossing a threshold. Moving below the threshold is considered the "false" value
        /// and above is considered the "true" value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="providername"></param>
        /// <param name="variable"></param>
        /// <param name="threshold"></param>
        /// <param name="falseval"></param>
        /// <param name="trueval"></param>
        /// <param name="category"></param>
        protected void AddThresholdEvent<T>(string providername, string variable, T threshold, string falseval, string trueval, MRADSEvent.EventCategory category = MRADSEvent.EventCategory.Status)
        {
            AddEvent(providername, variable, true, (v, p, _) => ThresholdFunc(v, p, threshold, falseval, trueval), category);
        }

        protected void AddCUThresholdEvent<T>(string variable, T threshold, string falseval, string trueval)
        {
            AddCUEvent(variable, true, (v, p) => ThresholdFunc(v, p, threshold, falseval, trueval));
        }
    }
}

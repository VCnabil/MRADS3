using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MRADS2.Panels;
using MRADS2.Ships.ViewModel;

namespace MRADS2
{
    /// <summary>
    /// An abstract base class inherited and implemented by classes corresponding to specific ships
    /// </summary>
    public abstract class MRADSShip
    {
        // The ship configuration containing information about CAN providers
        public readonly MRADSShipConfig Config = new MRADSShipConfig();
        // The ship state which contains the value of the ship variables at a specific time
        public MRADSShipState State { get; private set; }

        // The ship view model which is used to bind to the panel views of the UI
        public DefaultBindVM ShipViewModel { get; private set; }

        public readonly List<MRADSControlUnit> ControlUnits = new List<MRADSControlUnit>();
        public readonly List<MRADSClutchPanel> ClutchPanels = new List<MRADSClutchPanel>();

        abstract protected void InitControlUnit(MRADSControlUnit CU);
        abstract protected void InitClutchPanel(MRADSClutchPanel CP);
        // Get the UI panels associated with this ship
        abstract public IEnumerable<BasePanel> GetPanels(MRADSDataVM vm);

        // A method to identify whether the ship data corresponds to this ship
        abstract protected bool CheckShipID(MRADSShip ship);

        abstract protected DefaultBindVM GetViewModel(MRADSDataVM vmship, MRADSShipConfig config);

        List<MRADSEventHandler> eventhandlers = new List<MRADSEventHandler>();

        virtual protected void InitShip()
        {
        }

        public void Init()
        {
            foreach (var cu in ControlUnits)
            {
                Config.AddProvider(cu);
                InitControlUnit(cu);
            }

            foreach (var cp in ClutchPanels)
            {
                Config.AddProvider(cp);
                InitClutchPanel(cp);
            }

            InitShip();

            State = new MRADSShipState(this);
        }

        public void SetShipViewModel(MRADSDataVM vmdata)
        {
            var vm = GetViewModel(vmdata, Config);

            if (vm != null)
                BindVM(vmdata, vm);

            ShipViewModel = vm;
        }

        /// <summary>
        /// Iterate through the ship properties and bind them to the view model
        /// </summary>
        /// <param name="vmship"></param>
        /// <param name="vm"></param>
        void BindVM(MRADSDataVM vmship, DefaultBindVM vm)
        {
            vm.DefaultBind(vmship);

            foreach (var prop in vm.GetType().GetProperties())
            {
                var val = prop.GetValue(vm);

                if (val is Array)
                {
                    foreach (var arrayval in (Array)val)
                    {
                        if (arrayval is DefaultBindVM)
                            BindVM(vmship, (DefaultBindVM)arrayval);
                    }
                }
                else
                {
                    if (val is DefaultBindVM)
                        BindVM(vmship, (DefaultBindVM)val);
                }
            }
        }

        protected void AddEventHandler(MRADSEventHandler handler)
        {
            eventhandlers.Add(handler);
        }

        public enum MessageType { First, Last, Standard }

        /// <summary>
        /// Check the message against the event handlers and add an event to the event collection if one is emitted
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        /// <param name="changes"></param>
        /// <returns></returns>
        public List<MRADSEvent> CheckEvents(MessageType type, MRADSDataImporter.MsgEntry msg, MRADSDataChangeSet changes = null)
        {
            List<MRADSEvent> events = new List<MRADSEvent>();

            foreach (var handler in eventhandlers)
            {
                IEnumerable<MRADSEvent> evts = null;

                switch (type)
                {
                    case MessageType.Standard:
                        evts = handler.EventCheck(State, msg, changes);
                        break;
                    // Check for first message events
                    case MessageType.First:
                        evts = handler.FirstMessage(State, msg);
                        break;
                    // Check for last message events
                    case MessageType.Last:
                        evts = handler.LastMessage(State, msg);
                        break;
                }

                if (evts != null)
                    events.AddRange(evts);
            }

            return (events);
        }

        /// <summary>
        /// Identify the appropriate MRADSShip derived class and instantiate it based on the MRADS data
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static MRADSShip GetShip(ImportSource source)
        {
            CANMessage msg = null;
            // Get a list of all of the derived classes
            var shiptypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(a => a.IsSubclassOf(typeof(MRADSShip)));
            List<MRADSShip> ships = new List<MRADSShip>();

            foreach (var t in shiptypes)
            {
                var ship = (MRADSShip)Activator.CreateInstance(t);
                
                ship.Init();

                ships.Add(ship);
            }

            source.GotoStart();

            while (true)
            {
                msg = source.ReadNext();
                if (msg == null)
                    break;

                foreach (var ship in ships)
                {
                    // Attempt to decode the message using this ship's decoders
                    MRADSDataProvider provider;
                    PGNDecoder decoder;
                    var values = ship.Config.Decode(msg.Source, msg.Channel, msg.PGN, msg.Data, out provider, out decoder);

                    if (values == null)
                        continue;

                    // Evolve this ship's state based on the new message
                    var changeset = new MRADSDataChangeSet(provider, decoder, msg.Time, null, values);
                    ship.State.UpdateState(changeset);

                    // Check if this data corresponds to this ship
                    if (ship.CheckShipID(ship))
                    {
                        source.GotoStart();

                        return (ship);
                    }
                }
            }

            source.GotoStart();

            return (null);
        }

        protected double StandardAInScale(int value) => value / 4095.0 * 5.0 * 101 / 100 - 1.0 / 20;
        protected double StandardAOutScale(int value) => value / 4095.0 * 2.5;
    }

    public class MRADSControlUnit : MRADSDataProvider
    {
        public object Data { get; }

        // Control units contain parameters
        public IEnumerable<MRADSVariableDefinition> Parameters => decoders.Values.Single(a => a is ParameterDecoder).GetVariableDefinitions();

        public MRADSControlUnit(string name, byte address, object data, params int[] channels) : base(name, address, channels)
        {
            Data = data;
        }
    }

    public class MRADSClutchPanel : MRADSDataProvider
    {
        public object Data { get; }

        public MRADSClutchPanel(string name, byte address, object data, params int[] channels) : base(name, address, channels)
        {
            Data = data;
        }
    }

    public class MRADS_AMUnit : MRADSDataProvider
    {
        public object Data { get; }
        public IEnumerable<MRADSVariableDefinition> Parameters => decoders.Values.Single(a => a is ParameterDecoder).GetVariableDefinitions();

        public MRADS_AMUnit(string name, byte address, object data, params int[] channels) : base(name, address, channels)
        {
            Data = data;
        }
    }

    /// <summary>
    /// The ship variable state at a given point in time
    /// </summary>
    public class MRADSShipState
    {
        public const int StaleMessageTimeSeconds = 5;

        // The state variables keyed by the CAN message provider and then by the name of the variable
        Dictionary<MRADSDataProvider, Dictionary<string, MRADSVariableValue>> statevars = new Dictionary<MRADSDataProvider, Dictionary<string, MRADSVariableValue>>();
        // The last time a given PGN was seen
        Dictionary<PGNDecoder, DateTime> lastsettimes = new Dictionary<PGNDecoder, DateTime>();
        // Keeps track of whether a provider is deemed offline or not
        Dictionary<MRADSDataProvider, bool> onlinestate = new Dictionary<MRADSDataProvider, bool>();
        MRADSShip ship;

        public DateTime Time { get; private set; }

        public bool AllVariablesSet
        {
            get
            {
                foreach (var p in statevars)
                {
                    if (!p.Value.All(a => a.Value.Set || a.Value.VariableDefinition.Decoder is ParameterDecoder || !a.Value.VariableDefinition.Required))
                        return (false);
                }

                return (true);
            }
        }

        public MRADSShipState(MRADSShip ship)
        {
            this.ship = ship;

            foreach (var provider in ship.Config.GetProviders())
            {
                // Add all of the providers listed in the ship configuration
                statevars[provider] = new Dictionary<string, MRADSVariableValue>();
                onlinestate[provider] = false;

                foreach (var vardef in provider.GetVariableDefinitions())
                {
                    // Internal variables are not mapped to CAN message data
                    if (vardef.Internal)
                    {
                        statevars[provider][vardef.Name] = new MRADSInternalValue(vardef, this);

                        continue;
                    }

                    statevars[provider][vardef.Name] = new MRADSVariableValue(vardef);

                    lastsettimes[vardef.Decoder] = DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// Make a copy of the current state
        /// </summary>
        /// <returns></returns>
        public MRADSShipState Clone()
        {
            MRADSShipState ret = new MRADSShipState(ship);

            ret.SetState(this);

            return (ret);
        }

        public bool ProviderOnline(MRADSDataProvider provider)
        {
            return (onlinestate[provider]);
        }
        
        public void ForceTime(DateTime time)
        {
            Time = time;
        }

        /// <summary>
        /// Check all variables against the stale message timer and mark them stale if necessary.
        /// This method will also determine the offline/online state of the providers
        /// </summary>
        public void ExpireVariables()
        {
            foreach (var kvp in onlinestate)
                onlinestate[kvp.Key] = false;

            foreach (var kvp in lastsettimes)
            {
                if ((Time - kvp.Value).TotalSeconds > StaleMessageTimeSeconds)
                {
                    foreach (var v in kvp.Key.GetVariableDefinitions())
                        statevars[kvp.Key.Provider][v.Name].Clear();
                }
                else
                    onlinestate[kvp.Key.Provider] = true;
            }
        }

        public void Reset()
        {
            foreach (var p in statevars.Values)
            {
                foreach (var v in p.Values)
                    v.Clear();
            }

            foreach (var kvp in lastsettimes)
                lastsettimes[kvp.Key] = DateTime.MinValue;
        }

        /// <summary>
        /// Evolve the variable state based on a set of variable changes
        /// </summary>
        /// <param name="changes">The list of new variable values</param>
        /// <param name="settime">Update the ship state time</param>
        /// <param name="onlyupdateifunset">Only updates unset variable values if set to true</param>
        public void UpdateState(MRADSDataChangeSet changes, bool settime = true, bool onlyupdateifunset = false)
        {
            if (settime)
                Time = changes.Time;

            if (!onlyupdateifunset || lastsettimes[changes.Decoder] == DateTime.MinValue)
                lastsettimes[changes.Decoder] = changes.Time;

            foreach (var v in changes.Changes)
            {
                SetValue(changes.Provider, v, onlyupdateifunset);

                // Updating the time also implicitly calls for setting the offline/online state of the provider.
                // Maybe it shouldn't be this way?
                if (settime)
                    onlinestate[changes.Provider] = true;
            }
        }

        /// <summary>
        /// Set this ship's state based on another given state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="settime"></param>
        /// <param name="onlyupdateifunset"></param>
        public void SetState(MRADSShipState state, bool settime = true, bool onlyupdateifunset = false)
        {
            if (settime)
                Time = state.Time;

            foreach (var kvp1 in statevars)
            {
                var provider = kvp1.Key;

                foreach (var kvp2 in kvp1.Value)
                {
                    var varname = kvp2.Key;
                    var vlocal = statevars[provider][varname];
                    var vstate = state.statevars[provider][varname];

                    if (vstate.Set && (!onlyupdateifunset || !vlocal.Set))
                        vlocal.SetValue(vstate.GetValueBaseObj());
                    else
                        vlocal.Clear();
                }
            }

            foreach (var kvp in state.lastsettimes)
                lastsettimes[kvp.Key] = kvp.Value;

            foreach (var kvp in state.onlinestate)
                onlinestate[kvp.Key] = kvp.Value;
        }

        /// <summary>
        /// Get a ship variable's value by CAN source address, CAN channel and variable name
        /// </summary>
        /// <param name="source"></param>
        /// <param name="channel"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public MRADSVariableValue GetValue(byte source, int channel, string name)
        {
            Dictionary<string, MRADSVariableValue> p;
            MRADSVariableValue ret;

            if (!statevars.TryGetValue(ship.Config.GetProvider(source, channel, name), out p))
                throw new Exception($"Source address not found: {source:X2}");
            if (!p.TryGetValue(name, out ret))
                throw new Exception($"Variable not found: {name}");

            return (ret);
        }

        public MRADSVariableValue GetValue(MRADSDataProvider provider, string name)
        {
            MRADSVariableValue ret;

            if (!statevars[provider].TryGetValue(name, out ret))
                throw new Exception($"Variable not found: {name}");

            return (ret);
        }

        public bool IsVariableSet(byte source, int channel, string name)
        {
            Dictionary<string, MRADSVariableValue> p;
            MRADSVariableValue ret;

            if (!statevars.TryGetValue(ship.Config.GetProvider(source, channel, name), out p))
                throw new Exception($"Source address not found: {source:X2}");
            if (!p.TryGetValue(name, out ret))
                throw new Exception($"Variable not found: {name}");

            return (ret.Set);
        }

        public void SetValue(MRADSDataProvider provider, MRADSVariableValue value, bool onlyupdateifunset = false)
        {
            Dictionary<string, MRADSVariableValue> p;

            if (!statevars.TryGetValue(provider, out p))
                throw new Exception($"Provider not found: {provider.Name}");

            if (!onlyupdateifunset || !p[value.VariableDefinition.Name].Set)
                p[value.VariableDefinition.Name].SetValue(value.GetValueBaseObj());
        }

        public void PrintState()
        {
            Trace.WriteLine(Time);

            foreach (var p in statevars)
            {
                var name = p.Key.Name;

                foreach (var v in p.Value)
                    Trace.WriteLine($"{name} ({p.Key.SourceAddresses.Select(a => "0x" + a.ToString("X2")).Aggregate((a, b) => a + ", " + b)}): {v.Value}");
            }
        }

        public List<(MRADSDataProvider Provider, MRADSVariableValue Value)> GetAllVariables()
        {
            List<(MRADSDataProvider Provider, MRADSVariableValue Value)> ret = new List<(MRADSDataProvider Provider, MRADSVariableValue Value)>();

            foreach (var p in statevars)
            {
                var provider = p.Key;

                foreach (var v in p.Value)
                    ret.Add((provider, v.Value));
            }

            return (ret);
        }
    }

    /// <summary>
    /// A class containing the CAN message providers on the ship and any arbitrary attributes
    /// </summary>
    public class MRADSShipConfig
    {
        List<MRADSDataProvider> providers = new List<MRADSDataProvider>();
        Dictionary<string, ShipAttribute> attributes = new Dictionary<string, ShipAttribute>();

        public MRADSDataProvider CreateProvider(string name, byte address, params int[] channels)
        {
            var provider = new MRADSDataProvider(name, address, channels);

            providers.Add(provider);

            return (provider);
        }

        public MRADSDataProvider CreateProvider(string name, byte[] addresses, params int[] channels)
        {
            var provider = new MRADSDataProvider(name, addresses, channels);

            providers.Add(provider);

            return (provider);
        }

        public void AddProvider(MRADSDataProvider provider)
        {
            providers.Add(provider);
        }

        public MRADSDataProvider GetProvider(string name)
        {
            return (providers.SingleOrDefault(a => a.Name == name));
        }

        public MRADSDataProvider GetProvider(byte address, int channel, ushort pgn)
        {
            IEnumerable<MRADSDataProvider> p;

            p = providers.Where(a => a.SourceAddresses.Contains(address));
            if (p.Count() == 1)
                return (p.ElementAt(0));

            p = p.Where(a => a.CANChannels.Contains(channel));
            if (p.Count() == 1)
                return (p.ElementAt(0));

            p = p.Where(a => a.HandlesPGN(pgn, address));
            if (p.Count() == 1)
                return (p.ElementAt(0));

            return (null);
        }

        public MRADSDataProvider GetProvider(byte address, int channel, string varname)
        {
            IEnumerable<MRADSDataProvider> p;

            p = providers.Where(a => a.SourceAddresses.Contains(address));
            if (p.Count() == 1)
                return (p.ElementAt(0));

            p = p.Where(a => a.CANChannels.Contains(channel));
            if (p.Count() == 1)
                return (p.ElementAt(0));

            p = p.Where(a => a.HandlesVariable(varname));
            if (p.Count() == 1)
                return (p.ElementAt(0));

            return (null);
        }

        public void SetAttribute(string name, int value)
        {
            attributes[name] = new ShipAttribute(value);
        }

        public T GetAttribute<T>(string name)
        {
            return (attributes[name].GetValue<T>());
        }

        public IEnumerable<MRADSDataProvider> GetProviders()
        {
            return (providers.Cast<MRADSDataProvider>());
        }

        public List<MRADSVariableValue> Decode(byte source, int channel, ushort pgn, byte[] data, out MRADSDataProvider provider, out PGNDecoder decoder)
        {
            decoder = null;

            provider = GetProvider(source, channel, pgn);
            if (provider == null)
                return (null);

            decoder = provider.GetDecoder(pgn, source);
            if (decoder == null)
                return (null);

            return (decoder.Decode(data));
        }
    }

    public class ShipAttribute
    {
        object val;

        public ShipAttribute(int value)
        {
            val = value;
        }

        public T GetValue<T>()
        {
            return ((T)val);
        }
    }

    /// <summary>
    /// A CAN message provider. Providers can be associated with multiple source addresses and
    /// multiple CAN channels. This class also holds the decoder for the messages associated with this provider
    /// </summary>
    public class MRADSDataProvider
    {
        public string Name { get; }
        public byte[] SourceAddresses { get; }
        public int[] CANChannels { get; }
        public MRADSVariableDefinition Online { get; private set; }

        // Decoders are associated with a key created from the PGN and the source address
        protected Dictionary<uint, PGNDecoder> decoders = new Dictionary<uint, PGNDecoder>();

        InternalDecoder internaldecoder;

        public MRADSDataProvider(string name, byte address, params int[] channels)
        {
            Name = name;
            SourceAddresses = new byte[] { address };
            CANChannels = channels;

            InitInternal();
        }

        public MRADSDataProvider(string name, byte[] addresses, params int[] channels)
        {
            Name = name;
            SourceAddresses = addresses;
            CANChannels = channels;

            InitInternal();
        }

        /// <summary>
        /// Adds an "internal" variable associated with the provider being online or offline. This variable is not directly
        /// associated with CAN message data
        /// </summary>
        void InitInternal()
        {
            internaldecoder = new InternalDecoder(this);

            Online = MRADSVariableDefinition.CreateInternal(MRADSVariableDefinition.VarType.Bool, "ProviderOnline", s => s.ProviderOnline(this));

            internaldecoder.AddVariableDefinition(Online);

            decoders.Add(uint.MaxValue, internaldecoder);
        }

        public InternalDecoder GetInternalDecoder() => internaldecoder;

        public PGNDecoder GetDecoder(ushort pgn, byte source)
        {
            var key = FormKey(pgn, source);

            if (!decoders.ContainsKey(key))
                return (null);

            return (decoders[key]);
        }

        uint FormKey(ushort pgn, byte source)
        {
            return ((uint)((pgn << 8) | source));
        }

        public PGNDecoder AddPGN(ushort pgn)
        {
            var decoder = new PGNDecoder(this, pgn);

            foreach (var src in SourceAddresses)
                decoders[FormKey(pgn, src)] = decoder;

            return (decoder);
        }

        public PGNDecoder AddPGN(ushort pgn, byte source)
        {
            var decoder = new PGNDecoder(this, pgn);

            decoders[FormKey(pgn, source)] = decoder;

            return (decoder);
        }

        public ParameterDecoder AddParameterPGN(ushort pgn)
        {
            var decoder = new ParameterDecoder(this, pgn);

            foreach (var src in SourceAddresses)
                decoders[FormKey(pgn, src)] = decoder;

            return (decoder);
        }

        public List<MRADSVariableDefinition> GetVariableDefinitions()
        {
            List<MRADSVariableDefinition> ret = new List<MRADSVariableDefinition>();

            foreach (var decoder in decoders)
                ret.AddRange(decoder.Value.GetVariableDefinitions());

            return (ret);
        }

        public MRADSVariableDefinition GetVariableDefinition(string name)
        {
            MRADSVariableDefinition ret = null;

            foreach (var decoder in decoders)
            {
                ret = decoder.Value.GetVariableDefinition(name);
                if (ret != null)
                    break;
            }

            return (ret);
        }

        public bool HandlesPGN(ushort pgn, byte source)
        {
            return (decoders.ContainsKey(FormKey(pgn, source)));
        }

        public bool HandlesVariable(string name)
        {
            return (decoders.Any(a => a.Value.HandlesVariable(name)));
        }

        public override string ToString()
        {
            return $"{Name} [{SourceAddresses.Select(a => a.ToString("X2")).Aggregate((a, b) => a + " " + b)}]";
        }
    }

    /// <summary>
    /// Decoder for the data within a CAN message specified by PGN
    /// </summary>
    public class PGNDecoder
    {
        public ushort PGN { get; }
        public MRADSDataProvider Provider { get; }

        protected List<MRADSVariableDefinition> vardefs = new List<MRADSVariableDefinition>();

        public PGNDecoder(MRADSDataProvider provider, ushort pgn)
        {
            Provider = provider;
            PGN = pgn;
        }

        public virtual void AddVariableDefinition(MRADSVariableDefinition vardef)
        {
            vardef.Decoder = this;

            vardefs.Add(vardef);
        }

        public virtual IEnumerable<MRADSVariableDefinition> GetVariableDefinitions()
        {
            return (vardefs.Cast<MRADSVariableDefinition>());
        }

        public virtual List<MRADSVariableValue> Decode(byte[] data)
        {
            List<MRADSVariableValue> ret = new List<MRADSVariableValue>();

            foreach (var v in vardefs)
                ret.Add(v.Decode(data));

            return (ret);
        }

        public bool HandlesVariable(string name)
        {
            return (vardefs.Any(a => a.Name == name));
        }

        public MRADSVariableDefinition GetVariableDefinition(string name)
        {
            return (vardefs.SingleOrDefault(a => a.Name == name));
        }
    }

    /// <summary>
    /// A specialized decoder for control unit parameter values
    /// </summary>
    public class ParameterDecoder : PGNDecoder
    {
        Dictionary<int, MRADSVariableDefinition> parametermap = new Dictionary<int, MRADSVariableDefinition>();

        public ParameterDecoder(MRADSDataProvider provider, ushort pgn) : base(provider, pgn)
        {
        }

        public void SetParameter(int index, string name)
        {
            if (parametermap.ContainsKey(index))
            {
                vardefs.Remove(parametermap[index]);
                parametermap.Remove(index);
            }

            MRADSVariableDefinition vardef = MRADSVariableDefinition.CreateInt(name, 0, 1);
            
            vardef.Decoder = this;

            vardefs.Add(vardef);

            parametermap.Add(index, vardef);
        }

        public override void AddVariableDefinition(MRADSVariableDefinition vardef)
        {
            throw new Exception("Use SetParameter to add parameters to the decoder");
        }

        /*public override IEnumerable<MRADSVariableDefinition> GetVariableDefinitions()
        {
            return (new MRADSVariableDefinition[0]);
        }*/

        public override List<MRADSVariableValue> Decode(byte[] data)
        {
            byte index = data[0];
            int i;
            List<MRADSVariableValue> ret = new List<MRADSVariableValue>();

            for (i = 2; i < data.Length; i += 2, index++)
            {
                if (!parametermap.ContainsKey(index))
                    continue;

                var vardef = parametermap[index];
                MRADSVariableValue val = new MRADSVariableValue(vardef);

                val.SetValue(BitConverter.ToInt16(data, i));

                ret.Add(val);
            }

            return (ret);
        }
    }

    public class InternalDecoder : PGNDecoder
    {
        public InternalDecoder(MRADSDataProvider provider) : base(provider, 0)
        {
        }

        public void UpdateInternalVars(MRADSShipState state)
        {

        }
    }

    /// <summary>
    /// The definition for a ship variable
    /// 
    /// Ship variables can be either booleans, integers (1, 2 or 4 byte) or floating point values (single or double precision).
    /// Variables can be decoded based on their start and ending bytes in the message or start byte and a bitindex. Additionally,
    /// a user function may be provided for more complex decoding cases.
    /// </summary>
    public class MRADSVariableDefinition
    {
        public enum VarType { Bool, Integer, Float };

        public string Name { get; private set; }
        public VarType Type { get; private set; }

        public PGNDecoder Decoder { get; set; }

        // Required indicates whether this variable must have a value for the ship's state to be considered "complete".
        // In most cases this should be true, however, for infrequently sent messages this can be set to false
        public bool Required { get; private set; }

        public bool Internal => funcinternal != null;

        // Scaling in this case refers to mapping ADC values back to voltages
        public bool CanScale => scalefunc != null;

        int startbyte, endbyte;
        int bitindex;
        bool bitinvert;

        Func<byte[], object> func = null;
        Func<MRADSShipState, object> funcinternal = null;
        Func<int, double> scalefunc = null;

        MRADSVariableDefinition()
        {
        }

        // Static methods to create variable definitions

        public static MRADSVariableDefinition CreateInt(string name, int startbyte, int endbyte, bool required = true)
        {
            MRADSVariableDefinition ret = new MRADSVariableDefinition();

            ret.Name = name;
            ret.Type = VarType.Integer;
            ret.Required = required;

            ret.startbyte = startbyte;
            ret.endbyte = endbyte;

            return (ret);
        }

        public static MRADSVariableDefinition CreateInt(string name, int startbyte, int endbyte, Func<int, double> scalefunc)
        {
            MRADSVariableDefinition ret = new MRADSVariableDefinition();

            ret.Name = name;
            ret.Type = VarType.Integer;
            ret.Required = false;

            ret.startbyte = startbyte;
            ret.endbyte = endbyte;

            ret.scalefunc = scalefunc;

            return (ret);
        }

        public static MRADSVariableDefinition CreateInt(string name, Func<byte[], object> func, bool required = true)
        {
            MRADSVariableDefinition ret = new MRADSVariableDefinition();

            ret.Name = name;
            ret.Type = VarType.Integer;
            ret.Required = required;

            ret.func = func;

            return (ret);
        }

        public static MRADSVariableDefinition CreateFloat(string name, int startbyte, int endbyte, bool required = true)
        {
            MRADSVariableDefinition ret = new MRADSVariableDefinition();

            ret.Name = name;
            ret.Type = VarType.Float;
            ret.Required = required;

            ret.startbyte = startbyte;
            ret.endbyte = endbyte;

            return (ret);
        }

        public static MRADSVariableDefinition CreateFloat(string name, Func<byte[], object> func, bool required = true)
        {
            MRADSVariableDefinition ret = new MRADSVariableDefinition();

            ret.Name = name;
            ret.Type = VarType.Float;
            ret.Required = required;

            ret.func = func;

            return (ret);
        }

        public static MRADSVariableDefinition CreateBool(string name, int byteindex, int bitindex, bool bitinvert = false, bool required = true)
        {
            MRADSVariableDefinition ret = new MRADSVariableDefinition();

            ret.Name = name;
            ret.Type = VarType.Bool;
            ret.Required = required;

            ret.startbyte = byteindex;
            ret.bitindex = bitindex;

            ret.bitinvert = bitinvert;

            return (ret);
        }

        public static MRADSVariableDefinition CreateBool(string name, Func<byte[], object> func, bool required = true)
        {
            MRADSVariableDefinition ret = new MRADSVariableDefinition();

            ret.Name = name;
            ret.Type = VarType.Bool;
            ret.Required = required;

            ret.func = func;

            return (ret);
        }

        public static MRADSVariableDefinition CreateInternal(VarType type, string name, Func<MRADSShipState, object> func)
        {
            MRADSVariableDefinition ret = new MRADSVariableDefinition();

            ret.Name = name;
            ret.Type = type;
            ret.Required = false;
            
            ret.funcinternal = func;

            return (ret);
        }

        public double ApplyScale(int value) => scalefunc(value);

        public object EvalInternal(MRADSShipState state)
        {
            if (funcinternal == null)
                throw new Exception("Not an internal variable");

            return (funcinternal(state));
        }

        public MRADSVariableValue Decode(byte[] data)
        {
            int sz;
            MRADSVariableValue ret = new MRADSVariableValue(this);

            if (func != null)
            {
                ret.SetValue(func(data));

                return (ret);
            }

            switch (Type)
            {
                case VarType.Bool:
                    bool t;

                    t = (data[startbyte] & (1 << bitindex)) != 0;

                    if (bitinvert)
                        ret.SetValue(!t);
                    else
                        ret.SetValue(t);

                    break;
                case VarType.Integer:
                    sz = endbyte - startbyte + 1;
                    int val;

                    if (sz == 1)
                        val = data[startbyte];
                    else if (sz == 2)
                        val = (int)BitConverter.ToUInt16(data, startbyte);
                    else if (sz == 4)
                        val = (int)BitConverter.ToUInt32(data, startbyte);
                    else
                        throw new Exception("Unhandled integer size");

                    ret.SetValue(val);

                    break;
                case VarType.Float:
                    sz = endbyte - startbyte + 1;

                    if (sz == 4)
                        ret.SetValue(BitConverter.ToSingle(data, startbyte));
                    else if (sz == 8)
                        ret.SetValue(BitConverter.ToDouble(data, startbyte));
                    else
                        throw new Exception("Unhandled floating point size");

                    break;
            }

            return (ret);
        }
    }

    /// <summary>
    /// The actual value of a ship variable
    /// </summary>
    public class MRADSVariableValue
    {
        public static bool ApplyScale = false;

        public MRADSVariableDefinition VariableDefinition { get; }
        object val;

        // Set indicates whether the value is valid or not
        public virtual bool Set { get; private set; }

        public MRADSVariableDefinition.VarType Type
        {
            get
            {
                if (ApplyScale && VariableDefinition.CanScale)
                    return (MRADSVariableDefinition.VarType.Float);
                else
                    return (VariableDefinition.Type);
            }
        }

        public MRADSVariableValue(MRADSVariableDefinition vardef)
        {
            VariableDefinition = vardef;

            Clear();
        }

        public void Clear()
        {
            switch (VariableDefinition.Type)
            {
                case MRADSVariableDefinition.VarType.Bool:
                    val = false;
                    break;
                case MRADSVariableDefinition.VarType.Integer:
                    val = 0;
                    break;
                case MRADSVariableDefinition.VarType.Float:
                    val = 0.0;
                    break;
            }

            Set = false;
        }

        public void SetValue(int value)
        {
            if (VariableDefinition.Type != MRADSVariableDefinition.VarType.Integer)
                throw new Exception($"Variable \"{VariableDefinition.Name}\" expects an integer value");

            val = value;

            Set = true;
        }

        public void SetValue(bool value)
        {
            if (VariableDefinition.Type != MRADSVariableDefinition.VarType.Bool)
                throw new Exception($"Variable \"{VariableDefinition.Name}\" expects a boolean value");

            val = value;

            Set = true;
        }

        public void SetValue(double value)
        {
            if (VariableDefinition.Type != MRADSVariableDefinition.VarType.Float)
                throw new Exception($"Variable \"{VariableDefinition.Name}\" expects a floating point value");

            val = value;

            Set = true;
        }

        public void SetValue(object value)
        {
            if (VariableDefinition.Type == MRADSVariableDefinition.VarType.Integer && !(value is int))
                throw new Exception($"Variable \"{VariableDefinition.Name}\" expects an integer value");
            else if (VariableDefinition.Type == MRADSVariableDefinition.VarType.Bool && !(value is bool))
                throw new Exception($"Variable \"{VariableDefinition.Name}\" expects a boolean value");
            else if (VariableDefinition.Type == MRADSVariableDefinition.VarType.Float && !(value is double))
                throw new Exception($"Variable \"{VariableDefinition.Name}\" expects a floating point value");

            val = value;

            Set = true;
        }

        public virtual double GetValue()
        {
            if (ApplyScale && VariableDefinition.CanScale)
                return (VariableDefinition.ApplyScale((int)val));
            else
                return ((double)val);
        }

        public virtual T GetValue<T>()
        {
            return ((T)val);
        }

        public virtual object GetValueObj()
        {
            if (ApplyScale && VariableDefinition.CanScale)
                return (VariableDefinition.ApplyScale((int)val));
            else
                return (val);
        }

        public virtual object GetValueBaseObj()
        {
            return (val);
        }

        public bool Equal(MRADSVariableValue b)
        {
            if (b.VariableDefinition != VariableDefinition)
                return (false);

            if (!b.Set)
                return (false);

            return (EqualValue(b.val));
        }

        public bool EqualValue(object value)
        {
            if (!Set)
                return (false);

            if (VariableDefinition.Type == MRADSVariableDefinition.VarType.Integer && !(value is int))
                throw new Exception($"Variable \"{VariableDefinition.Name}\" expects an integer value");
            else if (VariableDefinition.Type == MRADSVariableDefinition.VarType.Bool && !(value is bool))
                throw new Exception($"Variable \"{VariableDefinition.Name}\" expects a boolean value");
            else if (VariableDefinition.Type == MRADSVariableDefinition.VarType.Float && !(value is double))
                throw new Exception($"Variable \"{VariableDefinition.Name}\" expects a floating point value");

            switch (VariableDefinition.Type)
            {
                case MRADSVariableDefinition.VarType.Bool:
                    return ((bool)val == (bool)value);
                case MRADSVariableDefinition.VarType.Integer:
                    return ((int)val == (int)value);
                case MRADSVariableDefinition.VarType.Float:
                    return ((double)val == (double)value);
            }

            return (false);
        }

        public override string ToString()
        {
            var pval = !Set ? "Not set" : val;

            return ($"{VariableDefinition.Name} = {pval}");
        }

        public MRADSVariableValue Clone()
        {
            MRADSVariableValue ret = new MRADSVariableValue(VariableDefinition);

            if (Set)
                ret.SetValue(val);

            return (ret);
        }
    }

    public class MRADSInternalValue : MRADSVariableValue
    {
        MRADSShipState state;

        public override bool Set => true;

        public MRADSInternalValue(MRADSVariableDefinition vardef, MRADSShipState state) : base(vardef)
        {
            this.state = state;
        }

        public override T GetValue<T>()
        {
            return ((T)VariableDefinition.EvalInternal(state));
        }

        public override object GetValueObj()
        {
            return (VariableDefinition.EvalInternal(state));
        }
    }
}

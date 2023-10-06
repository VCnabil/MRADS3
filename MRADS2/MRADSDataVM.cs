using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using MRADS2.Controls;

namespace MRADS2
{
    /// <summary>
    /// The MRADS data view model for the main window
    /// </summary>
    public class MRADSDataVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<VariableVM> Variables { get; } = new ObservableCollection<VariableVM>();

        public ObservableCollection<MRADSEvent> AllEvents { get; } = new ObservableCollection<MRADSEvent>();
        public ObservableCollection<MRADSEvent> FaultEvents { get; } = new ObservableCollection<MRADSEvent>();
        public ObservableCollection<MRADSEvent> WarningEvents { get; } = new ObservableCollection<MRADSEvent>();
        public ObservableCollection<MRADSEvent> StatusEvents { get; } = new ObservableCollection<MRADSEvent>();
        public ObservableCollection<MRADSEvent> OnlineOfflineEvents { get; } = new ObservableCollection<MRADSEvent>();

        public DateTime Time
        {
            get => data.Ship.State.Time;
            set => SetNextTime(value);
        }

        public double StartTimeSeconds => (data.Source.Start - DateTime.UnixEpoch).TotalSeconds;
        public double EndTimeSeconds => (data.Source.End - DateTime.UnixEpoch).TotalSeconds;
        public double TimeSeconds
        {
            get
            {
                return ((Time - DateTime.UnixEpoch).TotalSeconds);
            }

            set
            {
                Time = DateTimeOffset.FromUnixTimeMilliseconds((long)(value * 1000)).DateTime;
            }
        }

        // Determines whether ADC values should be displayed in ADC counts or voltages
        public bool ScalingMode
        {
            get => MRADSVariableValue.ApplyScale;
            set
            {
                MRADSVariableValue.ApplyScale = value;

                StateUpdateCallback(true);

                UpdatePlotTypes();

                OnPropertyChanged();
            }
        }

        MRADSData data;

        public MRADSShip Ship => data.Ship;

        public PlotControl Plotter { get; set; } = null;

        public bool Updating { get; private set; } = false;
        DateTime nexttime = DateTime.MinValue;

        List<(string plotname, VariableVM varvm, ChartEntry chart)> plotvars = new List<(string plotname, VariableVM varvm, ChartEntry chart)>();

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public MRADSDataVM(MRADSData data, PlotControl plotter)
        {
            this.data = data;

            var vars = data.Ship.State.GetAllVariables();

            // Add all the ship variables
            foreach (var v in vars)
                Variables.Add(new VariableVM(v.Provider, v.Value));

            foreach (var ev in data.Source.Events)
            {
                switch (ev.Category)
                {
                    case MRADSEvent.EventCategory.Fault:
                        FaultEvents.Add(ev);
                        break;
                    case MRADSEvent.EventCategory.OnlineOffline:
                        OnlineOfflineEvents.Add(ev);
                        break;
                    case MRADSEvent.EventCategory.Status:
                        StatusEvents.Add(ev);
                        break;
                    case MRADSEvent.EventCategory.Warning:
                        WarningEvents.Add(ev);
                        break;
                }

                AllEvents.Add(ev);
            }
            
            // Set up the data plotter
            Plotter = plotter;

            if (Plotter != null)
            {
                Plotter.CursorMoveHandler = HandleCursorMove;
                Plotter.RemovePlotCallback = (name) => RemovePlot(plotvars.SingleOrDefault(a => a.plotname == name).varvm);

                Plotter.XStart = data.Source.BufferStart;
                Plotter.XEnd = data.Source.BufferEnd;

                UpdatePlots();
            }

            data.Source.StateUpdateCallback = StateUpdateCallback;
        }

        public VariableVM GetVariable(string name)
        {
            return (Variables.SingleOrDefault(a => a.Name == name));
        }

        public VariableVM GetVariable(string provider, string name)
        {
            return (Variables.SingleOrDefault(a => a.Source == provider && a.Name == name));
        }

        public VariableVM GetVariable(MRADSVariableDefinition vardef)
        {
            return (Variables.SingleOrDefault(a => a.Definition == vardef));
        }

        public BindVariables<T> MultiBind<T>(Func<Dictionary<string, VariableVM>, T> func, params string[] names)
        {
            var variables = Variables.Where(a => names.Contains(a.Name));

            if (variables.Count() < names.Length)
                throw new Exception("Not all variables found in multibind");

            return (new BindVariables<T>(variables, func));
        }

        public BindVariables<T> MultiBind<T>(string provider, Func<Dictionary<string, VariableVM>, T> func, params string[] names)
        {
            var variables = Variables.Where(a => a.Source == provider && names.Contains(a.Name));

            if (variables.Count() < names.Length)
                throw new Exception("Not all variables found in multibind");

            return (new BindVariables<T>(variables, func));
        }

        VariableVM FromPlotName(string name)
        {
            string[] sp = name.Split(": ");

            return (GetVariable(sp[0], sp[1].Replace(" ", "")));
        }

        string PlotName(VariableVM varvm)
        {
            return (PlotName(varvm.Definition));
        }

        /// <summary>
        /// Form a plot name from a variable definition. It will attempt to split
        /// a variable name up based on capitalization within the name
        /// </summary>
        /// <param name="vardef"></param>
        /// <returns></returns>
        string PlotName(MRADSVariableDefinition vardef)
        {
            int i;
            string nm = "";

            if (vardef.Name.Contains(' '))
                nm = vardef.Name;
            else
            {
                for (i = 0; i < vardef.Name.Length; i++)
                {
                    if (i > 0 && char.IsUpper(vardef.Name[i]) && (!char.IsUpper(vardef.Name[i - 1]) || (i < (vardef.Name.Length - 1) && !char.IsUpper(vardef.Name[i + 1]))))
                        nm += " ";

                    nm += vardef.Name[i];
                }
            }

            return (vardef.Decoder.Provider.Name + ": " + nm);
        }

        public IEnumerable<string> PlotterCharts => Plotter == null ? null : Plotter.ChartNames;
        
        /// <summary>
        /// Find existing charts compatible with this variable
        /// </summary>
        /// <param name="varvm"></param>
        /// <returns></returns>
        public HashSet<string> CompatibleCharts(VariableVM varvm)
        {
            HashSet<string> ret = new HashSet<string>();

            foreach (var pv in plotvars)
            {
                if (pv.varvm.Definition.Type == varvm.Definition.Type)
                    ret.Add(pv.chart.Name);
            }

            return (ret);
        }

        public string AddPlot(VariableVM varvm, string chartname = null, bool draw = true)
        {
            return AddPlots(new VariableVM[] { varvm }, chartname, draw);
        }

        /// <summary>
        /// Create new plots from given variables. If a chartname is specified then add them to that chart,
        /// otherwise create a new chart.
        /// </summary>
        /// <param name="varvms"></param>
        /// <param name="chartname"></param>
        /// <param name="draw"></param>
        /// <returns></returns>
        public string AddPlots(IEnumerable<VariableVM> varvms, string chartname = null, bool draw = true)
        {
            ChartEntry chartentry = null;

            if (Plotter == null)
                return (null);

            // Ensure that all variables are of the same plot type
            if (varvms.Select(a => GetPlotType(a)).Distinct().Count() > 1)
                return null;

            foreach (var v in varvms)
            {
                if (v.PlotState.Plotting)
                    continue;

                var color = Plotter.GetNextPlotColor();

                var plotname = PlotName(v);
                chartentry = Plotter.AddPlot(plotname, GetPlotType(v), color, chartentry == null ? chartname : chartentry.Name);

                if (v.Definition.Name == "ProviderOnline") // Hack
                    chartentry.Chart.PlotTimeout = double.MaxValue;
                else
                    chartentry.Chart.PlotTimeout = MRADSSettings.CANTimeout;

                plotvars.Add((plotname, v, chartentry));

                if (draw)
                    UpdatePlots();

                v.SetPlotState(true, color);

                RunPlotChangeCBs(v, true);
            }

            return (chartentry.Name);
        }

        public void RemovePlot(VariableVM varvm)
        {
            if (Plotter == null)
                return;

            var plotvar = plotvars.SingleOrDefault(a => a.varvm == varvm);
            plotvars.Remove(plotvar);

            Plotter.RemovePlot(plotvar.plotname, true);

            varvm.SetPlotState(false, Colors.Transparent);

            RunPlotChangeCBs(varvm, false);
        }

        public void RemoveAllPlots()
        {
            while (plotvars.Count > 0)
                RemovePlot(plotvars[0].varvm);
        }

        /// <summary>
        /// Set whether the cursor is enabled on the charts or not
        /// </summary>
        /// <param name="enable"></param>
        public void SetCursorEnable(bool enable)
        {
            if (Plotter != null)
                Plotter.CursorEnable = enable;
        }

        Dictionary<VariableVM, List<Action<VariableVM, bool>>> plotchangecbs = new Dictionary<VariableVM, List<Action<VariableVM, bool>>>();

        void RunPlotChangeCBs(VariableVM varvm, bool plotadded)
        {
            if (!plotchangecbs.ContainsKey(varvm))
                return;

            foreach (var cb in plotchangecbs[varvm])
                cb.Invoke(varvm, plotadded);
        }

        /// <summary>
        /// Register for a callback when a plot for a variable is created or removed
        /// </summary>
        /// <param name="varvm"></param>
        /// <param name="callback"></param>
        public void RegisterPlotChangeCB(VariableVM varvm, Action<VariableVM, bool> callback)
        {
            if (!plotchangecbs.ContainsKey(varvm))
                plotchangecbs[varvm] = new List<Action<VariableVM, bool>>();

            plotchangecbs[varvm].Add(callback);
        }

        public Color GetPlotColor(VariableVM varvm)
        {
            return (Plotter.GetPlotColor(PlotName(varvm)));
        }

        LineChart.PlotTypes GetPlotType(VariableVM varvm, bool internaltype = false)
        {
            LineChart.PlotTypes plottype = LineChart.PlotTypes.Logic;
            MRADSVariableDefinition.VarType type = internaltype ? varvm.Definition.Type : varvm.Type;

            switch (type)
            {
                case MRADSVariableDefinition.VarType.Bool:
                    plottype = LineChart.PlotTypes.Logic;
                    break;
                case MRADSVariableDefinition.VarType.Integer:
                    plottype = LineChart.PlotTypes.Integer;
                    break;
                case MRADSVariableDefinition.VarType.Float:
                    plottype = LineChart.PlotTypes.Float;
                    break;
            }

            return (plottype);
        }

        /// <summary>
        /// Update all of the plots with the current data. This should ideally only be called when the plot window changes
        /// or a plot has been added
        /// </summary>
        void UpdatePlots()
        {
            if (plotvars.Count == 0)
            {
                Plotter.Redraw();

                return;
            }

            var changes = data.Source.GetDataAtCurrentTime(plotvars.Select(a => a.varvm.Definition));

            Plotter.DrawingFreeze = true;

            foreach (var kvp in changes)
            {
                if (kvp.Value.Count == 0)
                    continue;

                DataPointCollection points = new DataPointCollection(PlotName(kvp.Key));

                foreach (var p in kvp.Value)
                {
                    double v = 0;

                    switch (p.Value.Type)
                    {
                        case MRADSVariableDefinition.VarType.Bool:
                            v = p.Value.GetValue<bool>() ? 1 : 0;
                            break;
                        case MRADSVariableDefinition.VarType.Integer:
                            v = p.Value.GetValue<int>();
                            break;
                        case MRADSVariableDefinition.VarType.Float:
                            v = p.Value.GetValue();
                            break;
                    }

                    points.AddPoint(p.Time, v);
                }

                Plotter.SetData(PlotName(kvp.Key), points);
            }

            Plotter.DrawingFreeze = false;
            Plotter.Redraw();
        }

        void UpdatePlotTypes()
        {
            foreach (var pv in plotvars)
            {
                if (pv.chart.Chart.PlotType != GetPlotType(pv.varvm))
                    pv.chart.Chart.PlotType = GetPlotType(pv.varvm);
            }
        }

        /// <summary>
        /// Readjust the data window to match the requested time and optionally recenter the window
        /// on this time
        /// </summary>
        /// <param name="value"></param>
        /// <param name="recenter"></param>
        void SetTime(DateTime value, bool recenter = true)
        {
            Updating = true;

            if (value < data.Source.Start)
                data.Source.SetTime(data.Source.Start, recenter);
            else if (value > data.Source.End)
                data.Source.SetTime(data.Source.End, recenter);
            else
                data.Source.SetTime(value, recenter);
        }

        public void SetNextTime(DateTime value)
        {
            if (!Updating)
            {
                SetTime(value);

                return;
            }

            nexttime = value;
        }

        /// <summary>
        /// Callback called when the MRADS data thread has finished collecting the data
        /// for the new time window
        /// </summary>
        /// <param name="bufferadjust"></param>
        void StateUpdateCallback(bool bufferadjust)
        {
            foreach (var v in Variables)
                v.Update();

            if (Plotter != null && bufferadjust)
            {
                Plotter.DrawingFreeze = true;
                Plotter.XStart = data.Source.BufferStart;
                Plotter.XEnd = data.Source.BufferEnd;
                Plotter.DrawingFreeze = false;

                UpdatePlots();

                Plotter.CursorPosition = Ship.State.Time;
            }

            // Temporarily set the internal timer tick rate to 1 ms for a smoother update
            WinApi.TimeBeginPeriod(1);

            OnPropertyChanged(nameof(Time));
            OnPropertyChanged(nameof(TimeSeconds));

            WinApi.TimeEndPeriod(1);

            Updating = false;

            // If there is a pending next time request, set it
            if (nexttime != DateTime.MinValue)
            {
                SetTime(nexttime);

                nexttime = DateTime.MinValue;
            }
        }

        void HandleCursorMove(DateTime newtime)
        {
            SetTime(newtime, false);
        }

        public void Close()
        {
            RemoveAllPlots();

            plotchangecbs.Clear();

            Plotter.CursorMoveHandler = null;
            Plotter.RemovePlotCallback = null;

            data.Source.StateUpdateCallback = null;

            Variables.Clear();
        }
    }

    public abstract class DataVMProvider
    {
        public MRADSDataVM DataVM { get; }

        public DataVMProvider(MRADSDataVM datavm)
        {
            DataVM = datavm;
        }
    }

    public interface VariableVMProvider
    {
        public VariableVM[] Variables { get; }
    }

    /// <summary>
    /// A ship variable view model
    /// </summary>
    public class VariableVM : INotifyPropertyChanged
    {
        public string Source => provider.Name;
        public string Name => value.VariableDefinition.Name;
        public string Value => !Set ? "Not set" : value.GetValueObj().ToString();
        public object RawValue => value.GetValueObj();
        public bool Set => value.Set;
        public MRADSVariableDefinition.VarType Type => value.Type;

        public MRADSVariableDefinition Definition => value.VariableDefinition;

        public class PlottingState
        {
            public bool Plotting;
            public Color PlotColor;
        }

        public PlottingState PlotState { get; } = new PlottingState() { Plotting = false, PlotColor = Colors.Transparent };

        MRADSDataProvider provider;
        MRADSVariableValue value;

        public event PropertyChangedEventHandler PropertyChanged;

        public VariableVM(MRADSDataProvider provider, MRADSVariableValue value)
        {
            this.provider = provider;
            this.value = value;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void SetPlotState(bool plotting, Color plotcolor)
        {
            PlotState.PlotColor = plotcolor;
            PlotState.Plotting = plotting;

            OnPropertyChanged(nameof(PlotState));
        }

        public void Update()
        {
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(RawValue));
        }

        public BindVariable<T> Bind<T>(Func<VariableVM, T> func)
        {
            return (new BindVariable<T>(this, func));
        }

        public BindVariable Bind()
        {
            return (new BindVariable(this));
        }

        public override string ToString()
        {
            return ($"{Name} = {Value}");
        }
    }

    /// <summary>
    /// A bound variable with the ability to map a variable to an arbitrary type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindVariable<T> : INotifyPropertyChanged, VariableVMProvider
    {
        public T Value
        {
            get
            {
                if (!Variable.Set)
                {
                    if (typeof(T) == typeof(string))
                        return ((dynamic)"---");
                    else
                        return (default(T));
                }
                else
                {
                    var val = func(Variable);

                    if (typeof(T) == typeof(string) && val == null)
                        return ((dynamic)"---");
                    else
                        return (val);
                }
            }
        }

        public VariableVM[] Variables { get; }
        public VariableVM Variable => Variables[0];

        Func<VariableVM, T> func;

        public BindVariable(VariableVM variable, Func<VariableVM, T> func)
        {
            this.func = func;
            Variables = new VariableVM[] { variable };

            variable.PropertyChanged += Variable_PropertyChanged;
        }

        private void Variable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Value");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// A collection of bound variables with the ability to map them to an arbitrary type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindVariables<T> : INotifyPropertyChanged, VariableVMProvider
    {
        public T Value
        {
            get
            {
                if (variables.Any(a => !a.Value.Set))
                {
                    if (typeof(T) == typeof(string))
                        return ((dynamic)"---");
                    else
                        return (default(T));
                }
                else
                {
                    var val = func(variables);

                    if (typeof(T) == typeof(string) && val == null)
                        return ((dynamic)"---");
                    else
                        return (val);
                }
            }
        }

        public VariableVM[] Variables => variables.Values.ToArray();

        Func<Dictionary<string, VariableVM>, T> func;
        Dictionary<string, VariableVM> variables = new Dictionary<string, VariableVM>();

        public BindVariables(IEnumerable<VariableVM> variables, Func<Dictionary<string, VariableVM>, T> func)
        {
            this.func = func;

            foreach (var v in variables)
            {
                this.variables[v.Name] = v;

                v.PropertyChanged += Variable_PropertyChanged;
            }
        }

        private void Variable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Value");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// A simple bound variable. The panel view components are bound to these objects from
    /// the ship's view model
    /// </summary>
    public class BindVariable : INotifyPropertyChanged, VariableVMProvider
    {
        public object Value => !Variable.Set ? "---" : Variable.RawValue;

        public VariableVM[] Variables { get; }
        public VariableVM Variable => Variables[0];

        public BindVariable(VariableVM variable)
        {
            Variables = new VariableVM[] { variable };

            variable.PropertyChanged += Variable_PropertyChanged;
        }

        private void Variable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Value");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // An abstract plot/chart interface. Right now there is only one implementation (MultiChart)
    public interface PlotControl
    {
        public DateTime XStart { get; set; }
        public DateTime XEnd { get; set; }

        public bool DrawingFreeze { get; set; }

        public bool CursorEnable { get; set; }

        public IEnumerable<string> ChartNames { get; }
        public LineChart GetChart(string name);

        public ChartEntry AddPlot(string plotname, LineChart.PlotTypes plottype, Color plotcolor, string chartname = null);
        public void RemovePlot(string plotname, bool removeemptyentries, string chartname = null);
        public Color GetPlotColor(string name);
        public Color GetNextPlotColor();

        public void SetData(string name, DataPointCollection points);

        public void Redraw();

        public Action<DateTime> CursorMoveHandler { get; set; }
        public Action<string> RemovePlotCallback { get; set; }

        public DateTime CursorPosition { get; set; }
    }

    public static class WinApi
    {
        /// <summary>TimeBeginPeriod(). See the Windows API documentation for details.</summary>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]

        public static extern uint TimeBeginPeriod(uint uMilliseconds);

        /// <summary>TimeEndPeriod(). See the Windows API documentation for details.</summary>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]

        public static extern uint TimeEndPeriod(uint uMilliseconds);
    }
}

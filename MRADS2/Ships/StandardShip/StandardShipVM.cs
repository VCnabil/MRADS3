using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRADS2.Ships.GenericViewModels;
using MRADS2.Ships.ViewModel;

namespace MRADS2.Ships.StandardShip
{
    public class StandardShipVM : DataVMProvider, DefaultBindVM
    {
        public ControlUnitVM[] ControlUnits { get; }
        public ClutchPanelVM[] ClutchPanels { get; }

        public ObservableCollection<CUParameterVM> CUParameters { get; private set; }

        public GPSVM GPS { get; }
        public VMUVM VMU { get; }

        public LCDVM LCD { get; }

        public StandardShipVM(MRADSDataVM datavm, MRADSShipConfig config) : base(datavm)
        {
            ControlUnits = new ControlUnitVM[2];
            ControlUnits[0] = new ControlUnitVM(datavm.Ship.ControlUnits[0], config);
            ControlUnits[1] = new ControlUnitVM(datavm.Ship.ControlUnits[1], config);

            ClutchPanels = new ClutchPanelVM[2];
            ClutchPanels[0] = new ClutchPanelVM(datavm.Ship.ClutchPanels[0]);
            ClutchPanels[1] = new ClutchPanelVM(datavm.Ship.ClutchPanels[1]);

            GPS = new GPSVM();
            VMU = new VMUVM();

            LCD = new LCDVM();
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            CUParameters = new ObservableCollection<CUParameterVM>(
                ControlUnits.SelectMany(a => a.ControlUnit.Parameters).GroupBy(a => a.Name, a => datavm.GetVariable(a), (key, v) => new CUParameterVM(key, v.ToArray())));
        }
    }

    public class CUParameterVM
    {
        public string Name { get; }
        public VariableVM[] Values { get; }

        public CUParameterVM(string name, VariableVM[] values)
        {
            Name = name;
            Values = values;
        }
    }

    public class WaterJetVM : DefaultBindVM
    {
        public BucketVM Bucket { get; } = new BucketVM();
        public NozzleVM Nozzle { get; } = new NozzleVM();
    }

    public class BucketVM : DefaultBindVM
    {
        public BindVariable ScaledFeedback { get; set; }
        public BindVariable FeedbackAnalogInput { get; set; }

        public BindVariable FeedbackFault { get; set; }
        public BindVariable NFUFault { get; set; }

        public BindVariable ValveOutput { get; set; }
    }

    public class NozzleVM : DefaultBindVM
    {
        public BindVariable ScaledFeedback { get; set; }
        public BindVariable FeedbackAnalogInput { get; set; }

        public BindVariable FeedbackFault { get; set; }
        public BindVariable NFUFault { get; set; }

        public BindVariable ValveOutput { get; set; }
    }

    public class EngineVM : DefaultBindVM
    {
        public BindVariable ScaledCommand { get; set; }

        public BindVariable ClutchPermissive { get; set; }
    }

    public class ControlUnitVM : DefaultBindVM
    {
        public WaterJetVM[] WaterJets { get; }
        public EngineVM[] Engines { get; }
        public StationVM[] Stations { get; }
        public StationTransferInfo StationTransfer { get; }
        public CUFaultsVM Faults { get; }

        public BindVariable CombinedSeparateAnalogInput { get; private set; }
        public BindVariable PortOrStbdLeverAnalogInput { get; private set; }
        public BindVariable<string> OnOffRemoteLeverModeState { get; private set; }
        public BindVariable<string> OnOffCombinedModeState { get; private set; }
        public BindVariable<string> PortOrStbdLeverSwitchState { get; private set; }

        public BindVariable<string> StationInControl { get; private set; }

        public BindVariable MessageCounter { get; private set; }
        public BindVariable PowerUpComplete { get; private set; }
        public BindVariables<string> SoftwareVersion { get; private set; }

        public BindVariables<string> CalibrationFeedback { get; private set; }

        public MRADSControlUnit ControlUnit;

        public ControlUnitVM(MRADSControlUnit controlunit, MRADSShipConfig config)
        {
            int i;

            ControlUnit = controlunit;

            WaterJets = new WaterJetVM[config.GetAttribute<int>("WaterJetsPerSide")];
            for (i = 0; i < WaterJets.Length; i++)
                WaterJets[i] = new WaterJetVM();

            Engines = new EngineVM[config.GetAttribute<int>("EnginesPerSide")];
            for (i = 0; i < Engines.Length; i++)
                Engines[i] = new EngineVM();

            Stations = new StationVM[4];
            Stations[0] = new StationVM("Station1", ControlUnit.Name);
            Stations[1] = new StationVM("Station2", ControlUnit.Name);
            Stations[2] = new StationVM("Station3", ControlUnit.Name);
            Stations[3] = new StationVM("Remote", ControlUnit.Name, false);

            StationTransfer = new StationTransferInfo(ControlUnit.Name);

            Faults = new CUFaultsVM(ControlUnit.Name);
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            var prefixes = (string[])ControlUnit.Data;
            string prefix1, prefix2;

            prefix1 = prefixes[0];
            prefix2 = prefixes[1];

            Engines[0].ScaledCommand = datavm.GetVariable(ControlUnit.Name, $"{prefix1}ScaledEngineCommand").Bind();
            Engines[0].ClutchPermissive = datavm.GetVariable(ControlUnit.Name, $"{prefix1}ClutchPermissiveState").Bind();
            Engines[1].ScaledCommand = datavm.GetVariable(ControlUnit.Name, $"{prefix2}ScaledEngineCommand").Bind();
            Engines[1].ClutchPermissive = datavm.GetVariable(ControlUnit.Name, $"{prefix2}ClutchPermissiveState").Bind();

            // Outboard on port (A), inboard on starboard (B)
            WaterJets[0].Bucket.ScaledFeedback = datavm.GetVariable(ControlUnit.Name, $"{prefix1}ScaledBucketFeedback").Bind();
            WaterJets[0].Bucket.FeedbackAnalogInput = datavm.GetVariable(ControlUnit.Name, $"{prefix1}BucketFeedbackAnalogInput").Bind();
            WaterJets[0].Bucket.FeedbackFault = datavm.GetVariable(ControlUnit.Name, $"{prefix1}BucketFdbkFault").Bind();
            WaterJets[0].Bucket.NFUFault = datavm.GetVariable(ControlUnit.Name, $"{prefix1}BucketNFUFault").Bind();
            WaterJets[0].Bucket.ValveOutput = datavm.GetVariable(ControlUnit.Name, $"{prefix1}BucketValveOutput").Bind();
            WaterJets[0].Nozzle.ScaledFeedback = datavm.GetVariable(ControlUnit.Name, $"{prefix1}ScaledNozzleFeedback").Bind();
            WaterJets[0].Nozzle.FeedbackAnalogInput = datavm.GetVariable(ControlUnit.Name, $"{prefix1}NozzleFeedbackAnalogInput").Bind();
            WaterJets[0].Nozzle.FeedbackFault = datavm.GetVariable(ControlUnit.Name, $"{prefix1}NozzleFdbkFault").Bind();
            WaterJets[0].Nozzle.NFUFault = datavm.GetVariable(ControlUnit.Name, $"{prefix1}NozzleNFUFault").Bind();
            WaterJets[0].Nozzle.ValveOutput = datavm.GetVariable(ControlUnit.Name, $"{prefix1}NozzleValveOutput").Bind();

            // Inboard on port (A), outboard on starboard (B)
            WaterJets[1].Bucket.ScaledFeedback = datavm.GetVariable(ControlUnit.Name, $"{prefix2}ScaledBucketFeedback").Bind();
            WaterJets[1].Bucket.FeedbackAnalogInput = datavm.GetVariable(ControlUnit.Name, $"{prefix2}BucketFeedbackAnalogInput").Bind();
            WaterJets[1].Bucket.FeedbackFault = datavm.GetVariable(ControlUnit.Name, $"{prefix2}BucketFdbkFault").Bind();
            WaterJets[1].Bucket.NFUFault = datavm.GetVariable(ControlUnit.Name, $"{prefix2}BucketNFUFault").Bind();
            WaterJets[1].Bucket.ValveOutput = datavm.GetVariable(ControlUnit.Name, $"{prefix2}BucketValveOutput").Bind();
            WaterJets[1].Nozzle.ScaledFeedback = datavm.GetVariable(ControlUnit.Name, $"{prefix2}ScaledNozzleFeedback").Bind();
            WaterJets[1].Nozzle.FeedbackAnalogInput = datavm.GetVariable(ControlUnit.Name, $"{prefix2}NozzleFeedbackAnalogInput").Bind();
            WaterJets[1].Nozzle.FeedbackFault = datavm.GetVariable(ControlUnit.Name, $"{prefix2}NozzleFdbkFault").Bind();
            WaterJets[1].Nozzle.NFUFault = datavm.GetVariable(ControlUnit.Name, $"{prefix2}NozzleNFUFault").Bind();
            WaterJets[1].Nozzle.ValveOutput = datavm.GetVariable(ControlUnit.Name, $"{prefix2}NozzleValveOutput").Bind();

            CombinedSeparateAnalogInput = datavm.GetVariable(ControlUnit.Name, $"CombinedSeparateAnalogInput").Bind();
            PortOrStbdLeverAnalogInput = datavm.GetVariable(ControlUnit.Name, $"PortOrStbdLeverAnalogInput").Bind();
            OnOffCombinedModeState = datavm.GetVariable(ControlUnit.Name, $"OnOffCombinedModeState").Bind(v => (bool)v.RawValue ? "Combined" : "Separate");
            PortOrStbdLeverSwitchState = datavm.GetVariable(ControlUnit.Name, $"PortOrStbdLeverSwitchState").Bind(v => (bool)v.RawValue ? "Port" : "Starboard");
            OnOffRemoteLeverModeState = datavm.GetVariable(ControlUnit.Name, $"OnOffRemoteLeverModeState").Bind(v => (bool)v.RawValue ? "On" : "Off");

            StationInControl = datavm.GetVariable(ControlUnit.Name, "StationInControl").Bind(v => GetStationInControlName((int)v.RawValue));

            MessageCounter = datavm.GetVariable(ControlUnit.Name, $"MessageCounter").Bind();
            PowerUpComplete = datavm.GetVariable(ControlUnit.Name, $"UnitPowerUpComplete").Bind();
            SoftwareVersion = datavm.MultiBind(ControlUnit.Name, v => $"{v["MajorSWVersion"].Value}.{v["MinorSWVersion"].Value} Rev{v["SWRevision"].Value}", "MajorSWVersion", "MinorSWVersion", "SWRevision");

            CalibrationFeedback = datavm.MultiBind(ControlUnit.Name,
                v => GetCalibrationFeedback((int)v["CalibrationFdbkType"].RawValue, (int)v["CalibrationFdbkOperation"].RawValue),
                "CalibrationFdbkType", "CalibrationFdbkOperation");
        }

        static public string GetStationInControlName(int index)
        {
            switch (index)
            {
                case 1:
                    return ("Bridge Center Station");
                case 2:
                    return ("Port Wing Station");
                case 3:
                    return ("Starboard Wing Station");
                default:
                    return ("Unknown");
            }
        }

        static readonly string[][] CalOperations = new string[][]
        {
            // Type 1
            new string[]
            {
                "Unknown",
                "Moving cylinders up/stbd during feedback calibration",
                "Moving cylinders down/port during feedback calibration",
                "Requesting user to [finish] or [abort] during feedback calibration",
                "Requesting user to move controls to detent during feedback calibration"
            },
            // Type 2
            new string[]
            {
                "Unknown",
                "Requesting user to move control inputs during control input calibration",
                "Requesting user to move control inputs to detent during control input calibration"
            },
            // Type 3
            new string[]
            {
                "Unknown",
                "Neutral bucket point was set"
            }
        };

        static public string GetCalibrationFeedback(int type, int op)
        {
            if (type > 3 || op >= CalOperations[type - 1].Length)
                return ("Unrecognized feedback");
            else if (type == 0 || op == 0)
                return (null);

            return (CalOperations[type - 1][op]);
        }
    }

    public class CUFaultsVM : DefaultBindVM
    {
        public BindVariable CalibrationFault { get; private set; }
        public BindVariable GeneralAlarm { get; private set; }
        public BindVariable ClutchPanelCommFault { get; private set; }

        string ControlUnit;

        public CUFaultsVM(string controlunit)
        {
            ControlUnit = controlunit;
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            CalibrationFault = datavm.GetVariable(ControlUnit, $"CalibrationFault").Bind();
            GeneralAlarm = datavm.GetVariable(ControlUnit, $"GeneralAlarm").Bind();
            ClutchPanelCommFault = datavm.GetVariable(ControlUnit, $"ClutchPanelCommFault").Bind();
        }
    }

    public class StationVM : DefaultBindVM
    {
        public BindVariable ThrottleAnalogInput { get; private set; }
        public BindVariable AzimuthAnalogInput { get; private set; }

        public BindVariable ThrustFault { get; private set; }
        public BindVariable AzimuthFault { get; private set; }

        string Name;
        string ControlUnit;
        bool ReportsFaults;

        public StationVM(string name, string controlunit, bool reportsfaults = true)
        {
            Name = name;
            ControlUnit = controlunit;
            ReportsFaults = reportsfaults;
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            ThrottleAnalogInput = datavm.GetVariable(ControlUnit, $"{Name}ThrottleAnalogInput").Bind();
            AzimuthAnalogInput = datavm.GetVariable(ControlUnit, $"{Name}AzimuthAnalogInput").Bind();

            if (ReportsFaults)
            {
                ThrustFault = datavm.GetVariable(ControlUnit, $"{Name}ThrustFault").Bind();
                AzimuthFault = datavm.GetVariable(ControlUnit, $"{Name}AzimuthFault").Bind();
            }
        }
    }

    public class StationTransferInfo : DefaultBindVM
    {
        public BindVariable MatchBuckets { get; private set; }
        public BindVariable MatchNozzles { get; private set; }
        public BindVariable AzimuthLever { get; private set; }
        public BindVariable ThrustLever { get; private set; }
        public BindVariable TransferPermissive { get; private set; }
        public BindVariable TransferUnderFault { get; private set; }

        string ControlUnit;

        public StationTransferInfo(string controlunit)
        {
            ControlUnit = controlunit;
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            MatchBuckets = datavm.GetVariable(ControlUnit, "StationTransferPendingMatchBuckets").Bind();
            MatchNozzles = datavm.GetVariable(ControlUnit, "StationTransferPendingMatchNozzles").Bind();

            AzimuthLever = datavm.GetVariable(ControlUnit, "StationTransferPendingTakeControlAzimuthLever").Bind();
            ThrustLever = datavm.GetVariable(ControlUnit, "StationTransferPendingTakeControlThrustLever").Bind();

            TransferPermissive = datavm.GetVariable(ControlUnit, "LocalStationTransferPermissive").Bind();
            TransferUnderFault = datavm.GetVariable(ControlUnit, "LocalStationTransferUnderFault").Bind();
        }
    }

    public class LCDVM : DefaultBindVM
    {
        public BindVariable<string> BackupAutoStateStarboardOutboard { get; private set; }
        public BindVariable<string> BackupAutoStatePortOutboard { get; private set; }
        public BindVariable<string> BackupAutoStateStarboardInboard { get; private set; }
        public BindVariable<string> BackupAutoStatePortInboard { get; private set; }
        public BindVariable WJInterfaceModuleFault { get; private set; }
        public BindVariables<string> CalibrationCommand { get; private set; }

        public void DefaultBind(MRADSDataVM datavm)
        {
            BackupAutoStateStarboardOutboard = datavm.GetVariable("LCD", "BackupAutoStateStarboardOutboard").Bind(v => GetBackupStatus((bool)v.RawValue));
            BackupAutoStatePortOutboard = datavm.GetVariable("LCD", "BackupAutoStatePortOutboard").Bind(v => GetBackupStatus((bool)v.RawValue));
            BackupAutoStateStarboardInboard = datavm.GetVariable("LCD", "BackupAutoStateStarboardInboard").Bind(v => GetBackupStatus((bool)v.RawValue));
            BackupAutoStatePortInboard = datavm.GetVariable("LCD", "BackupAutoStatePortInboard").Bind(v => GetBackupStatus((bool)v.RawValue));
            WJInterfaceModuleFault = datavm.GetVariable("LCD", "WJInterfaceModuleFault").Bind();
            CalibrationCommand = datavm.MultiBind("LCD",
                v => GetCalibrationCommand((int)v["CalibrationCmdOperation"].RawValue, (int)v["CalibrationCmdDestination"].RawValue, (int)v["CalibrationCmdType"].RawValue),
                "CalibrationCmdOperation", "CalibrationCmdDestination", "CalibrationCmdType");
        }

        string GetBackupStatus(bool val)
        {
            if (val)
                return ("Auto");
            else
                return ("Backup");
        }

        static readonly string[] CalOperations = new string[] { "Unknown", "Start", "Finish", "Abort", "Capture" };
        static readonly string[] CalDestinations = new string[] { "Port", "Starboard", "Unknown" };
        static readonly string[] CalTypes = new string[] { "Unknown", "Feedback", "Control Input", "Neutral Bucket" };

        static public string GetCalibrationCommand(int op, int dest, int type)
        {
            string ret = "";

            if (op < CalOperations.Length)
                ret += CalOperations[op];
            else
                ret += "Unknown";

            ret += " ";

            if (dest < CalDestinations.Length)
                ret += CalDestinations[dest];
            else
                ret += "Unknown";

            ret += " Control Unit ";

            if (type < CalTypes.Length)
                ret += CalTypes[type];
            else
                ret += "Unknown";

            ret += " Calibration";

            return (ret);
        }
    }

    public class ClutchPanelVM : DefaultBindVM
    {
        public ClutchStateVM[] States { get; }

        public BindVariables<string> SoftwareVersion { get; private set; }
        public BindVariable CANRxFault { get; private set; }

        MRADSClutchPanel ClutchPanel;

        public ClutchPanelVM(MRADSClutchPanel clutchpanel)
        {
            ClutchPanel = clutchpanel;

            States = new ClutchStateVM[2];
            States[0] = new ClutchStateVM();
            States[1] = new ClutchStateVM();
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            var prefixes = (string[])ClutchPanel.Data;
            string prefix1, prefix2;

            prefix1 = prefixes[0];
            prefix2 = prefixes[1];

            // Outboard on port (A), inboard on starboard (B)
            States[0].EngagedFeedback = datavm.GetVariable(ClutchPanel.Name, $"{prefix1}ClutchEngagedFeedbackState").Bind();
            States[0].BackflushFeedback = datavm.GetVariable(ClutchPanel.Name, $"{prefix1}ClutchBackflushFeedbackState").Bind();
            States[0].EngagedLED = datavm.GetVariable(ClutchPanel.Name, $"{prefix1}ClutchPanelEngagedLEDState").Bind();
            States[0].BackflushLED = datavm.GetVariable(ClutchPanel.Name, $"{prefix1}ClutchPanelBackflushLEDState").Bind();

            // Inboard on port (A), outboard on starboard (B)
            States[1].EngagedFeedback = datavm.GetVariable(ClutchPanel.Name, $"{prefix2}ClutchEngagedFeedbackState").Bind();
            States[1].BackflushFeedback = datavm.GetVariable(ClutchPanel.Name, $"{prefix2}ClutchBackflushFeedbackState").Bind();
            States[1].EngagedLED = datavm.GetVariable(ClutchPanel.Name, $"{prefix2}ClutchPanelEngagedLEDState").Bind();
            States[1].BackflushLED = datavm.GetVariable(ClutchPanel.Name, $"{prefix2}ClutchPanelBackflushLEDState").Bind();

            CANRxFault = datavm.GetVariable(ClutchPanel.Name, "CANRxFault").Bind();
            SoftwareVersion = datavm.MultiBind(ClutchPanel.Name, v => $"{v["MajorSWVersion"].Value}.{v["MinorSWVersion"].Value} Rev{v["SWRevision"].Value}", "MajorSWVersion", "MinorSWVersion", "SWRevision");
        }
    }

    public class ClutchStateVM
    {
        public BindVariable EngagedFeedback { get; set; }
        public BindVariable EngagedLED { get; set; }

        public BindVariable BackflushFeedback { get; set; }
        public BindVariable BackflushLED { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRADS2.Ships.GenericViewModels;
using MRADS2.Ships.ViewModel;

namespace MRADS2.Ships.CCM
{
    public class CCMVM : DataVMProvider, DefaultBindVM
    {
        public EngineVM[] Engines { get; }
        public EngineControllerVM[] EngineControllers { get; }

        public DriveCUVM DriveCU { get; }
        public TabCUVM TabCU { get; }

        public ClutchPanelVM ClutchPanel { get; }

        public PanelInterfaceModuleVM PanelInterface { get; }

        public LCDVM LCD { get; }

        public VintageVMUVM VMUICEConsole { get; }
        public VintageVMUVM VMUHullConsole { get; }

        public PDMVM[] PDMs { get; }

        public CCMVM(MRADSDataVM datavm, MRADSShipConfig config, bool oldvmu = true) : base(datavm)
        {
            Engines = new EngineVM[2];
            Engines[0] = new EngineVM(config.GetProvider("Port Engine"));
            Engines[1] = new EngineVM(config.GetProvider("Starboard Engine"));

            EngineControllers = new EngineControllerVM[2];
            EngineControllers[0] = new EngineControllerVM(config.GetProvider("Port Engine Controller"));
            EngineControllers[1] = new EngineControllerVM(config.GetProvider("Starboard Engine Controller"));

            DriveCU = new DriveCUVM(datavm.Ship.ControlUnits[0]);
            TabCU = new TabCUVM(datavm.Ship.ControlUnits[1]);

            ClutchPanel = new ClutchPanelVM(datavm.Ship.ClutchPanels[0]);

            PanelInterface = new PanelInterfaceModuleVM(config.GetProvider("Panel Interface Module"));

            LCD = new LCDVM(config.GetProvider("Murphy LCD"));

            if (oldvmu)
            {
                VMUICEConsole = new VintageVMUVM(config.GetProvider("VMU ICE Console"));
                VMUHullConsole = new VintageVMUVM(config.GetProvider("VMU Hull Console"));
            }

            CCM ship = (CCM)datavm.Ship;

            PDMs = new PDMVM[ship.PDMs.Count];

            for (int i = 0; i < PDMs.Length; i++)
                PDMs[i] = new PDMVM(ship.PDMs[i]);
        }
    }

    public class EngineVM : DefaultBindVM
    {
        public BindVariable ActualRPM { get; private set; }
        public BindVariable DesiredRPM { get; private set; }

        public BindVariable Load { get; private set; }

        public BindVariable ClutchPressure { get; private set; }

        public BindVariables<string> GearCommand { get; private set; }

        public BindVariable TotalEngineHours { get; private set; }

        public BindVariable TransmissionOilTemperature { get; private set; }

        public BindVariable OilPressure { get; private set; }

        public BindVariable<bool> Timeout { get; private set; }

        MRADSDataProvider Engine;

        public EngineVM(MRADSDataProvider engine)
        {
            Engine = engine;
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            ActualRPM = datavm.GetVariable(Engine.Name, "EngineRPM").Bind();
            DesiredRPM = datavm.GetVariable(Engine.Name, "DesiredRPM").Bind();

            Load = datavm.GetVariable(Engine.Name, "EngineLoad").Bind();

            ClutchPressure = datavm.GetVariable(Engine.Name, "ClutchPressure").Bind();

            GearCommand = datavm.MultiBind(Engine.Name, v =>
            {
                if ((bool)v["CurrentGearAHD"].RawValue)
                    return ("Ahead");
                else if ((bool)v["CurrentGearREV"].RawValue)
                    return ("Reverse");
                else if ((bool)v["CurrentGearNEU"].RawValue)
                    return ("Neutral");
                else
                    return ("Unknown");
            }, "CurrentGearAHD", "CurrentGearREV", "CurrentGearNEU");

            TotalEngineHours = datavm.GetVariable(Engine.Name, "TotalEngineHours").Bind();

            TransmissionOilTemperature = datavm.GetVariable(Engine.Name, "TransmissionOilTemperature").Bind();

            OilPressure = datavm.GetVariable(Engine.Name, "EngineOilPressure").Bind();

            Timeout = datavm.GetVariable(Engine.Name, "ProviderOnline").Bind(v => !(bool)v.RawValue);
        }
    }

    public class EngineControllerVM : DefaultBindVM
    {
        public BindVariable ActualShaftRPM { get; private set; }

        public BindVariable<bool> Timeout { get; private set; }

        MRADSDataProvider EngineController;

        public EngineControllerVM(MRADSDataProvider enginecontroller)
        {
            EngineController = enginecontroller;
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            ActualShaftRPM = datavm.GetVariable(EngineController.Name, "ActualShaftRPM").Bind();

            Timeout = datavm.GetVariable(EngineController.Name, "ProviderOnline").Bind(v => !(bool)v.RawValue);
        }
    }

    public class DriveCUVM : DefaultBindVM
    {
        public DriveCUSideVM[] DriveCUSide { get; }

        public BindVariable<string> DockingTransitState { get; private set; }
        public BindVariable DockingZone { get; private set; }
        public BindVariable<string> StationInControl { get; private set; }

        public BindVariables<string> JoystickTillerStationSwitch { get; private set; }
        public BindVariables<string> ThrottleHelmStationSwitch { get; private set; }
        public BindVariable<string> DockTransitSwitch { get; private set; }
        public BindVariable<string> AutoTrimSwitch { get; private set; }

        public BindVariable TiebarAnalogInput { get; private set; }

        public BindVariable JoystickXAnalogInput { get; private set; }
        public BindVariable JoystickYAnalogInput { get; private set; }
        public BindVariable TillerAnalogInput { get; private set; }
        public BindVariable HelmAnalogInput { get; private set; }

        public BindVariable CalibrationFault { get; private set; }
        public BindVariable GeneralAlarm { get; private set; }
        public BindVariable DockmodeFault { get; private set; }
        public BindVariable TillerFault { get; private set; }
        public BindVariable JoyXFault { get; private set; }
        public BindVariable JoyYFault { get; private set; }
        public BindVariable HelmFault { get; private set; }

        public BindVariable MurphyLCDCommunicationTimeout { get; private set; }
        public BindVariable ClutchCommunicationTimeout { get; private set; }
        public BindVariable CANFault { get; private set; }
        public BindVariable PortEngineCommunicationTimeout { get; private set; }
        public BindVariable StarboardEngineCommunicationTimeout { get; private set; }
        public BindVariable PortEngineControllerCommunicationTimeout { get; private set; }
        public BindVariable StarboardEngineControllerCommunicationTimeout { get; private set; }

        public BindVariable DockmodeFaultDueToSensorFault { get; private set; }
        public BindVariable DockmodeFaultDueToOffTrollingSwitch { get; private set; }
        public BindVariable DockmodeFaultDueToLowRPM { get; private set; }
        public BindVariable DockmodeFaultDueToJoyXFault { get; private set; }
        public BindVariable DockmodeFaultDueToEngineCommunication { get; private set; }
        public BindVariable DockmodeFaultDueToEngineControllerCommunication { get; private set; }
        public BindVariable DockmodeFaultDueToClutchCommunication { get; private set; }
        public BindVariable DockmodeFaultDueToTillerFault { get; private set; }

        public BindVariable HighPortRPMClutchInterlock { get; private set; }
        public BindVariable HighStarboardRPMClutchInterlock { get; private set; }
        public BindVariable DisengagedPortClutchInterlock { get; private set; }
        public BindVariable DisengagedStarboardClutchInterlock { get; private set; }

        public BindVariables<string> SoftwareVersion { get; private set; }

        public BindVariable<bool> Timeout { get; private set; }

        MRADSControlUnit ControlUnit;

        public DriveCUVM(MRADSControlUnit controlunit)
        {
            ControlUnit = controlunit;

            DriveCUSide = new DriveCUSideVM[2];
            DriveCUSide[0] = new DriveCUSideVM(ControlUnit, "Port");
            DriveCUSide[1] = new DriveCUSideVM(ControlUnit, "Stbd");
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            DockingTransitState = datavm.GetVariable(ControlUnit.Name, "DockingTransitState").Bind(v => (bool)v.RawValue ? "Docking" : "Transit");
            DockingZone = datavm.GetVariable(ControlUnit.Name, "JoystickDockingzone").Bind();
            StationInControl = datavm.GetVariable(ControlUnit.Name, "StationInControl").Bind(v => StationInControlName((int)v.RawValue));

            JoystickTillerStationSwitch = datavm.MultiBind(ControlUnit.Name, v =>
            {
                bool joysticktillerswitch = (bool)v["JoystickTillerStationSwitch"].RawValue;
                bool throttlehelmswitch = (bool)v["ThrottleHelmStationSwitch"].RawValue;

                if (joysticktillerswitch && throttlehelmswitch)
                    return ("Standby");
                else if (joysticktillerswitch)
                    return ("Set");
                else
                    return ("Not Set");
            }, "JoystickTillerStationSwitch", "ThrottleHelmStationSwitch");
            ThrottleHelmStationSwitch = datavm.MultiBind(ControlUnit.Name, v =>
            {
                bool joysticktillerswitch = (bool)v["JoystickTillerStationSwitch"].RawValue;
                bool throttlehelmswitch = (bool)v["ThrottleHelmStationSwitch"].RawValue;

                if (joysticktillerswitch && throttlehelmswitch)
                    return ("Standby");
                else if (throttlehelmswitch)
                    return ("Set");
                else
                    return ("Not Set");
            }, "JoystickTillerStationSwitch", "ThrottleHelmStationSwitch");
            DockTransitSwitch = datavm.GetVariable(ControlUnit.Name, "DockTransitJoystickSwitch").Bind(v => (bool)v.RawValue ? "Docking" : "Transit");
            AutoTrimSwitch = datavm.GetVariable(ControlUnit.Name, "AutoTrimSwitch").Bind(v => (bool)v.RawValue ? "On" : "Off");

            TiebarAnalogInput = datavm.GetVariable(ControlUnit.Name, "TiebarAnalogInput").Bind();

            JoystickXAnalogInput = datavm.GetVariable(ControlUnit.Name, "JoystickXAnalogInput").Bind();
            JoystickYAnalogInput = datavm.GetVariable(ControlUnit.Name, "JoystickYAnalogInput").Bind();
            TillerAnalogInput = datavm.GetVariable(ControlUnit.Name, "TillerAnalogInput").Bind();
            HelmAnalogInput = datavm.GetVariable(ControlUnit.Name, "HelmAnalogInput").Bind();

            CalibrationFault = datavm.GetVariable(ControlUnit.Name, "CalibrationFault").Bind();
            GeneralAlarm = datavm.GetVariable(ControlUnit.Name, "GeneralAlarm").Bind();
            DockmodeFault = datavm.GetVariable(ControlUnit.Name, "DockmodeFault").Bind();
            TillerFault = datavm.GetVariable(ControlUnit.Name, "TillerFault").Bind();
            JoyXFault = datavm.GetVariable(ControlUnit.Name, "JoyXFault").Bind();
            JoyYFault = datavm.GetVariable(ControlUnit.Name, "JoyYFault").Bind();
            HelmFault = datavm.GetVariable(ControlUnit.Name, "HelmFault").Bind();

            MurphyLCDCommunicationTimeout = datavm.GetVariable(ControlUnit.Name, "MurphyLCDCommunicationTimeout").Bind();
            ClutchCommunicationTimeout = datavm.GetVariable(ControlUnit.Name, "ClutchCommunicationTimeout").Bind();
            CANFault = datavm.GetVariable(ControlUnit.Name, "CANFault").Bind();
            PortEngineCommunicationTimeout = datavm.GetVariable(ControlUnit.Name, "PortEngineCommunicationTimeout").Bind();
            StarboardEngineCommunicationTimeout = datavm.GetVariable(ControlUnit.Name, "StbdEngineCommunicationTimeout").Bind();
            PortEngineControllerCommunicationTimeout = datavm.GetVariable(ControlUnit.Name, "PortEngineControllerCommunicationTimeout").Bind();
            StarboardEngineControllerCommunicationTimeout = datavm.GetVariable(ControlUnit.Name, "StbdEngineControllerCommunicationTimeout").Bind();

            DockmodeFaultDueToSensorFault = datavm.GetVariable(ControlUnit.Name, "DockmodeFaultDueToSensorFault").Bind();
            DockmodeFaultDueToOffTrollingSwitch = datavm.GetVariable(ControlUnit.Name, "DockmodeFaultDueToOFFTrollingSwitch").Bind();
            DockmodeFaultDueToLowRPM = datavm.GetVariable(ControlUnit.Name, "DockmodeFaultDueToLowRPM").Bind();
            DockmodeFaultDueToJoyXFault = datavm.GetVariable(ControlUnit.Name, "DockmodeFaultDueToJoyXFault").Bind();
            DockmodeFaultDueToEngineCommunication = datavm.GetVariable(ControlUnit.Name, "DockmodeFaultDueToEngineCommunication").Bind();
            DockmodeFaultDueToEngineControllerCommunication = datavm.GetVariable(ControlUnit.Name, "DockmodeFaultDueToEngineControllerCommunication").Bind();
            DockmodeFaultDueToClutchCommunication = datavm.GetVariable(ControlUnit.Name, "DockmodeFaultDueToClutchCommunication").Bind();
            DockmodeFaultDueToTillerFault = datavm.GetVariable(ControlUnit.Name, "DockmodeFaultDueToTillerFault").Bind();

            HighPortRPMClutchInterlock = datavm.GetVariable(ControlUnit.Name, "HighPortRPMClutchInterlock").Bind();
            HighStarboardRPMClutchInterlock = datavm.GetVariable(ControlUnit.Name, "HighStbdRPMClutchInterlock").Bind();
            DisengagedPortClutchInterlock = datavm.GetVariable(ControlUnit.Name, "DisengagedPortClutchInterlock").Bind();
            DisengagedStarboardClutchInterlock = datavm.GetVariable(ControlUnit.Name, "DisengagedStbdClutchInterlock").Bind();

            SoftwareVersion = datavm.MultiBind(ControlUnit.Name, v => $"{v["MajorSWVersion"].Value}.{v["MinorSWVersion"].Value} Rev{v["SWRevision"].Value}", "MajorSWVersion", "MinorSWVersion", "SWRevision");

            Timeout = datavm.GetVariable(ControlUnit.Name, "ProviderOnline").Bind(v => !(bool)v.RawValue);
        }

        public static string StationInControlName(int v)
        {
            switch (v)
            {
                case 0:
                    return ("Joy/Tiller");
                case 1:
                    return ("Throttle/Helm");
                default:
                    return ("None");
            }
        }
    }

    public class DriveCUSideVM : DefaultBindVM
    {
        public BindVariable ScaledEngineCommand { get; private set; }
        public BindVariable ThrottleCommand { get; private set; }
        public BindVariables<string> ClutchCommand { get; private set; }

        public BindVariable DriveSteerFeedbackScaled { get; private set; }
        public BindVariable DriveSteerFeedbackRaw { get; private set; }
        public BindVariable DriveTrimFeedbackScaled { get; private set; }
        public BindVariable DriveTrimFeedbackRaw { get; private set; }

        public BindVariable NavigatorStationTrimAnalogInput { get; private set; }
        public BindVariable ForwardPanelTrimAnalogInput { get; private set; }
        public BindVariable ThrottleAnalogInput { get; private set; }

        public BindVariable DesiredTrollingShaftRPM { get; private set; }
        public BindVariable DesiredTrollingDutyCycle { get; private set; }
        public BindVariable MaxTrollingDutyCycle { get; private set; }
        public BindVariable MaxTrollingDutyCycleAdjustment { get; private set; }
        public BindVariable TrollingKI { get; private set; }

        public BindVariable TrimSensorFault { get; private set; }
        public BindVariable SteerSensorFault { get; private set; }
        public BindVariable TrimNFUFault { get; private set; }
        public BindVariable SteerNFUFault { get; private set; }
        public BindVariable NavigatorTrimJoystickFault { get; private set; }
        public BindVariable ForwardPanelTrimJoystickFault { get; private set; }
        public BindVariable LeverFault { get; private set; }

        public BindVariable EngineAnalogOutput { get; private set; }
        public BindVariable SteerAnalogOutput { get; private set; }
        public BindVariable TrimAnalogOutput { get; private set; }

        MRADSControlUnit ControlUnit;
        string Side;

        public DriveCUSideVM(MRADSControlUnit controlunit, string side)
        {
            ControlUnit = controlunit;
            Side = side;
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            ScaledEngineCommand = datavm.GetVariable(ControlUnit.Name, $"Scaled{Side}EngineCommand").Bind();
            ThrottleCommand = datavm.GetVariable(ControlUnit.Name, $"ScaledActive{Side}ThrottleControlInputCommand").Bind();
            ClutchCommand = datavm.MultiBind(ControlUnit.Name, v =>
            {
                if ((bool)v[$"Integrated{Side}ClutchAheadCommand"].RawValue)
                    return ("Ahead");
                else if ((bool)v[$"Integrated{Side}ClutchReverseCommand"].RawValue)
                    return ("Reverse");
                else
                    return ("Neutral");
            }, $"Integrated{Side}ClutchAheadCommand", $"Integrated{Side}ClutchReverseCommand");

            DriveSteerFeedbackScaled = datavm.GetVariable(ControlUnit.Name, $"Scaled{Side}DriveSteerFeedback").Bind();
            DriveSteerFeedbackRaw = datavm.GetVariable(ControlUnit.Name, $"{Side}SteerFeedbackAnalogInput").Bind();
            DriveTrimFeedbackScaled = datavm.GetVariable(ControlUnit.Name, $"Scaled{Side}DriveTrimFeedback").Bind();
            DriveTrimFeedbackRaw = datavm.GetVariable(ControlUnit.Name, $"{Side}TrimFeedbackAnalogInput").Bind();

            NavigatorStationTrimAnalogInput = datavm.GetVariable(ControlUnit.Name, $"NavigatorStation{Side}TrimAnalogInput").Bind();
            ForwardPanelTrimAnalogInput = datavm.GetVariable(ControlUnit.Name, $"FwdPanel{Side}TrimAnalogInput").Bind();
            ThrottleAnalogInput = datavm.GetVariable(ControlUnit.Name, $"{Side}ThrottleAnalogInput").Bind();

            DesiredTrollingShaftRPM = datavm.GetVariable(ControlUnit.Name, $"Desired{Side}TrollingShaftRPM").Bind();
            DesiredTrollingDutyCycle = datavm.GetVariable(ControlUnit.Name, $"Desired{Side}TrollingDutyCycle").Bind();
            MaxTrollingDutyCycle = datavm.GetVariable(ControlUnit.Name, $"Max{Side}TrollingDutyCycle").Bind();
            MaxTrollingDutyCycleAdjustment = datavm.GetVariable(ControlUnit.Name, $"Max{Side}TrollingDutyCycleAdjustment").Bind();
            TrollingKI = datavm.GetVariable(ControlUnit.Name, $"{Side}TrollingKI").Bind();

            TrimSensorFault = datavm.GetVariable(ControlUnit.Name, $"{Side}TrimSensorFault").Bind();
            SteerSensorFault = datavm.GetVariable(ControlUnit.Name, $"{Side}SteerSensorFault").Bind();
            TrimNFUFault = datavm.GetVariable(ControlUnit.Name, $"{Side}TrimNFUFault").Bind();
            SteerNFUFault = datavm.GetVariable(ControlUnit.Name, $"{Side}SteerNFUFault").Bind();
            NavigatorTrimJoystickFault = datavm.GetVariable(ControlUnit.Name, $"Navigator{Side}TrimJoystickFault").Bind();
            ForwardPanelTrimJoystickFault = datavm.GetVariable(ControlUnit.Name, $"FwdPanel{Side}TrimJoystickFault").Bind();
            LeverFault = datavm.GetVariable(ControlUnit.Name, $"{Side}LeverFault").Bind();

            EngineAnalogOutput = datavm.GetVariable(ControlUnit.Name, $"{Side}EngineAnalogOutput").Bind();
            SteerAnalogOutput = datavm.GetVariable(ControlUnit.Name, $"{Side}SteerAnalogOutput").Bind();
            TrimAnalogOutput = datavm.GetVariable(ControlUnit.Name, $"{Side}TrimAnalogOutput").Bind();
        }
    }

    public class TabCUVM : DefaultBindVM
    {
        public TabCUSideVM[] TabCUSide { get; }
        public BindVariable IntegratedSteeringActive { get; private set; }
        public BindVariable<string> StationInControl { get; private set; }

        public BindVariable<string> IntegratedSteeringSwitch { get; private set; }

        public BindVariable TillerAnalogInput { get; private set; }
        public BindVariable HelmAnalogInput { get; private set; }
        public BindVariable NavigatorRollJoystickAnalogInput { get; private set; }
        public BindVariable NavigatorTrimJoystickAnalogInput { get; private set; }
        public BindVariable ForwardPanelRollJoystickAnalogInput { get; private set; }
        public BindVariable ForwardPanelTrimJoystickAnalogInput { get; private set; }

        public BindVariable CalibrationFault { get; private set; }
        public BindVariable GeneralAlarm { get; private set; }
        public BindVariable NavigatorTrimJoystickFault { get; private set; }
        public BindVariable NavigatorRollJoystickFault { get; private set; }
        public BindVariable ForwardPanelTrimJoystickFault { get; private set; }
        public BindVariable ForwardPanelRollJoystickFault { get; private set; }
        public BindVariable TillerFault { get; private set; }
        public BindVariable HelmFault { get; private set; }

        public BindVariable DriveControlUnitCommunicationTimeout { get; private set; }
        public BindVariable ClutchCommunicationTimeout { get; private set; }
        public BindVariable CANFault { get; private set; }
        public BindVariable PortEngineCommunicationTimeout { get; private set; }
        public BindVariable StarboardEngineCommunicationTimeout { get; private set; }
        public BindVariable PortEngineControllerCommunicationTimeout { get; private set; }
        public BindVariable StarboardEngineControllerCommunicationTimeout { get; private set; }

        public BindVariables<string> SoftwareVersion { get; private set; }

        public BindVariable<bool> Timeout { get; private set; }

        MRADSControlUnit ControlUnit;

        public TabCUVM(MRADSControlUnit controlunit)
        {
            ControlUnit = controlunit;

            TabCUSide = new TabCUSideVM[2];
            TabCUSide[0] = new TabCUSideVM(ControlUnit, "Port");
            TabCUSide[1] = new TabCUSideVM(ControlUnit, "Stbd");
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            IntegratedSteeringActive = datavm.GetVariable(ControlUnit.Name, "IntegratedSteeringActive").Bind();
            StationInControl = datavm.GetVariable(ControlUnit.Name, "StationInControl").Bind(v => DriveCUVM.StationInControlName((int)v.RawValue));

            IntegratedSteeringSwitch = datavm.GetVariable(ControlUnit.Name, "IntegratedSteeringSwitch").Bind(v => (bool)v.RawValue ? "On" : "Off");

            TillerAnalogInput = datavm.GetVariable(ControlUnit.Name, "TillerAnalogInput").Bind();
            HelmAnalogInput = datavm.GetVariable(ControlUnit.Name, "HelmAnalogInput").Bind();
            NavigatorRollJoystickAnalogInput = datavm.GetVariable(ControlUnit.Name, "NavigatorRollJoystickAnalogInput").Bind();
            NavigatorTrimJoystickAnalogInput = datavm.GetVariable(ControlUnit.Name, "NavigatorTrimJoystickAnalogInput").Bind();
            ForwardPanelRollJoystickAnalogInput = datavm.GetVariable(ControlUnit.Name, "FwdPanelRollJoystickAnalogInput").Bind();
            ForwardPanelTrimJoystickAnalogInput = datavm.GetVariable(ControlUnit.Name, "FwdPanelTrimJoystickAnalogInput").Bind();

            CalibrationFault = datavm.GetVariable(ControlUnit.Name, "CalibrationFault").Bind();
            GeneralAlarm = datavm.GetVariable(ControlUnit.Name, "GeneralAlarm").Bind();
            NavigatorTrimJoystickFault = datavm.GetVariable(ControlUnit.Name, "NavigatorStationTrimJoystickFault").Bind();
            NavigatorRollJoystickFault = datavm.GetVariable(ControlUnit.Name, "NavigatorStationRollJoystickFault").Bind();
            ForwardPanelTrimJoystickFault = datavm.GetVariable(ControlUnit.Name, "FwdPanelTrimJoystickFault").Bind();
            ForwardPanelRollJoystickFault = datavm.GetVariable(ControlUnit.Name, "FwdPanelRollJoystickFault").Bind();
            TillerFault = datavm.GetVariable(ControlUnit.Name, "TillerFault").Bind();
            HelmFault = datavm.GetVariable(ControlUnit.Name, "HelmFault").Bind();

            DriveControlUnitCommunicationTimeout = datavm.GetVariable(ControlUnit.Name, "DriveControlUnitCommunicationTimeout").Bind();
            ClutchCommunicationTimeout = datavm.GetVariable(ControlUnit.Name, "ClutchCommunicationTimeout").Bind();
            CANFault = datavm.GetVariable(ControlUnit.Name, "CANFault").Bind();
            PortEngineCommunicationTimeout = datavm.GetVariable(ControlUnit.Name, "PortEngineCommunicationTimeout").Bind();
            StarboardEngineCommunicationTimeout = datavm.GetVariable(ControlUnit.Name, "StbdEngineCommunicationTimeout").Bind();
            PortEngineControllerCommunicationTimeout = datavm.GetVariable(ControlUnit.Name, "PortEngineControllerCommunicationTimeout").Bind();
            StarboardEngineControllerCommunicationTimeout = datavm.GetVariable(ControlUnit.Name, "StbdEngineControllerCommunicationTimeout").Bind();

            SoftwareVersion = datavm.MultiBind(ControlUnit.Name, v => $"{v["MajorSWVersion"].Value}.{v["MinorSWVersion"].Value} Rev{v["SWRevision"].Value}", "MajorSWVersion", "MinorSWVersion", "SWRevision");

            Timeout = datavm.GetVariable(ControlUnit.Name, "ProviderOnline").Bind(v => !(bool)v.RawValue);
        }
    }

    public class TabCUSideVM : DefaultBindVM
    {
        public BindVariable TabFeedbackScaled { get; private set; }
        public BindVariable TabFeedbackRaw { get; private set; }

        public BindVariable TabSensorFault { get; private set; }
        public BindVariable TabNFUFault { get; private set; }

        public BindVariable TabAnalogOutput { get; private set; }

        MRADSControlUnit ControlUnit;
        string Side;

        public TabCUSideVM(MRADSControlUnit controlunit, string side)
        {
            ControlUnit = controlunit;
            Side = side;
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            TabFeedbackScaled = datavm.GetVariable(ControlUnit.Name, $"Scaled{Side}TabFeedback").Bind();
            TabFeedbackRaw = datavm.GetVariable(ControlUnit.Name, $"{Side}TabAnalogInput").Bind();

            TabSensorFault = datavm.GetVariable(ControlUnit.Name, $"{Side}TabSensorFault").Bind();
            TabNFUFault = datavm.GetVariable(ControlUnit.Name, $"{Side}TabNFUFault").Bind();

            TabAnalogOutput = datavm.GetVariable(ControlUnit.Name, $"{Side}TabAnalogOutput").Bind();
        }
    }

    public class LCDVM : DefaultBindVM
    {
        public BindVariable BoatSpeed { get; private set; }
        public BindVariable<bool> Timeout { get; private set; }

        MRADSDataProvider LCD;

        public LCDVM(MRADSDataProvider lcd)
        {
            LCD = lcd;
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            BoatSpeed = datavm.GetVariable(LCD.Name, "BoatSpeed").Bind();
            Timeout = datavm.GetVariable(LCD.Name, "ProviderOnline").Bind(v => !(bool)v.RawValue);
        }
    }

    public class ClutchPanelVM : DefaultBindVM
    {
        public ClutchPanelSideVM[] ClutchSide { get; }

        public BindVariable<string> ManualSteeringSwitch { get; private set; }
        public BindVariable<string> TrollingSwitch { get; private set; }
        public BindVariable<bool> AutoBackupSwitch { get; private set; }

        public BindVariable DriveControlUnitCommunicationTimeout { get; private set; }

        public BindVariables<string> SoftwareVersion { get; private set; }

        public BindVariable<bool> Timeout { get; private set; }

        MRADSClutchPanel ClutchPanel;

        public ClutchPanelVM(MRADSClutchPanel clutchpanel)
        {
            ClutchPanel = clutchpanel;

            ClutchSide = new ClutchPanelSideVM[2];
            ClutchSide[0] = new ClutchPanelSideVM(ClutchPanel, "Port");
            ClutchSide[1] = new ClutchPanelSideVM(ClutchPanel, "Stbd");
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            ManualSteeringSwitch = datavm.GetVariable(ClutchPanel.Name, "IntegratedManualSwitch").Bind(v => (bool)v.RawValue ? "Integrated" : "Manual");
            TrollingSwitch = datavm.GetVariable(ClutchPanel.Name, "TrollingONOFFSwitch").Bind(v => (bool)v.RawValue ? "On" : "Off");
            AutoBackupSwitch = datavm.GetVariable(ClutchPanel.Name, "BackupAutoSwitch").Bind(v => !(bool)v.RawValue);

            DriveControlUnitCommunicationTimeout = datavm.GetVariable(ClutchPanel.Name, "DriveControlUnitCommunicationTimeout").Bind();

            SoftwareVersion = datavm.MultiBind(ClutchPanel.Name, v => $"{v["MajorSWVersion"].Value}.{v["MinorSWVersion"].Value} Rev{v["SWRevision"].Value}", "MajorSWVersion", "MinorSWVersion", "SWRevision");

            Timeout = datavm.GetVariable(ClutchPanel.Name, "ProviderOnline").Bind(v => !(bool)v.RawValue);
        }
    }

    public class ClutchPanelSideVM : DefaultBindVM
    {
        public BindVariables<string> Command { get; private set; }

        public BindVariables<string> Feedback { get; private set; }

        MRADSClutchPanel ClutchPanel;
        string Side;

        public ClutchPanelSideVM(MRADSClutchPanel clutchpanel, string side)
        {
            ClutchPanel = clutchpanel;
            Side = side;
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            Command = datavm.MultiBind(ClutchPanel.Name, v =>
            {
                if ((bool)v[$"{Side}EngageAHDCmd"].RawValue)
                    return ("Ahead");
                else if ((bool)v[$"{Side}EngageREVCmd"].RawValue)
                    return ("Reverse");
                else
                    return ("Neutral");
            }, $"{Side}EngageAHDCmd", $"{Side}EngageREVCmd");

            Feedback = datavm.MultiBind(ClutchPanel.Name, v =>
            {
                if ((bool)v[$"{Side}EngageAHDFeedback"].RawValue)
                    return ("Ahead");
                else if ((bool)v[$"{Side}EngageREVFeedback"].RawValue)
                    return ("Reverse");
                else
                    return ("Neutral");
            }, $"{Side}EngageAHDFeedback", $"{Side}EngageREVFeedback");
        }
    }

    public class PanelInterfaceModuleVM : DefaultBindVM
    {
        public BindVariable PortEngineAutoBackupSwitch { get; private set; }
        public BindVariable StarboardEngineAutoBackupSwitch { get; private set; }

        public BindVariable PortSteerAutoBackupSwitch { get; private set; }
        public BindVariable StarboardSteerAutoBackupSwitch { get; private set; }

        public BindVariable PortDriveAutoBackupSwitch { get; private set; }
        public BindVariable StarboardDriveAutoBackupSwitch { get; private set; }

        public BindVariable TabAutoBackupSwitch { get; private set; }

        public BindVariable<bool> Timeout { get; private set; }

        MRADSDataProvider PanelInterfaceModule;

        public PanelInterfaceModuleVM(MRADSDataProvider panelinterfacemodule)
        {
            PanelInterfaceModule = panelinterfacemodule;
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            PortEngineAutoBackupSwitch = datavm.GetVariable(PanelInterfaceModule.Name, "PortEngineAutoBackupSwitch").Bind();
            StarboardEngineAutoBackupSwitch = datavm.GetVariable(PanelInterfaceModule.Name, "StbdEngineAutoBackupSwitch").Bind();

            PortSteerAutoBackupSwitch = datavm.GetVariable(PanelInterfaceModule.Name, "PortSteerAutoBackupSwitch").Bind();
            StarboardSteerAutoBackupSwitch = datavm.GetVariable(PanelInterfaceModule.Name, "StbdSteerAutoBackupSwitch").Bind();

            PortDriveAutoBackupSwitch = datavm.GetVariable(PanelInterfaceModule.Name, "PortDriveAutoBackupSwitch").Bind();
            StarboardDriveAutoBackupSwitch = datavm.GetVariable(PanelInterfaceModule.Name, "StbdDriveAutoBackupSwitch").Bind();

            TabAutoBackupSwitch = datavm.GetVariable(PanelInterfaceModule.Name, "TabAutoBackupSwitch").Bind();

            Timeout = datavm.GetVariable(PanelInterfaceModule.Name, "ProviderOnline").Bind(v => !(bool)v.RawValue);
        }
    }

    public class PDMVM : DefaultBindVM
    {
        public string Name => PDM.Name;
        public List<BindVariable> InputVariables { get; } = new List<BindVariable>();
        public List<BindVariable> OutputVariables { get; } = new List<BindVariable>();

        public BindVariable<bool> Timeout { get; private set; }

        CCM.PDM PDM;

        public PDMVM(CCM.PDM pdm)
        {
            PDM = pdm;
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            foreach (var input in PDM.Inputs)
                InputVariables.Add(datavm.GetVariable(PDM.Provider.Name, input.VarName).Bind());

            foreach (var output in PDM.Outputs)
                OutputVariables.Add(datavm.GetVariable(PDM.Provider.Name, output.VarName).Bind());

            Timeout = datavm.GetVariable(PDM.Provider.Name, "ProviderOnline").Bind(v => !(bool)v.RawValue);
        }
    }
}

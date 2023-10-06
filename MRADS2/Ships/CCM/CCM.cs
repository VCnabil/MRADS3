using MRADS2.Panels;
using MRADS2.Ships.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRADS2.Panels.CCM;

namespace MRADS2.Ships.CCM
{
    public class CCM : MRADSShip
    {
        public List<PDM> PDMs = new List<PDM>();

        public override IEnumerable<BasePanel> GetPanels(MRADSDataVM vmdata)
        {
            List<BasePanel> ret = new List<BasePanel>();

            var shipvm = (CCMVM)ShipViewModel;

            ret.Add(new LCDIndicationPanel(vmdata, shipvm));
            ret.Add(new EngineFeedbackPanel(vmdata, shipvm));
            ret.Add(new StatePanel(vmdata, shipvm));
            ret.Add(new SwitchPositionsPanel(vmdata, shipvm));
            ret.Add(new FeedbacksPanel(vmdata, shipvm));
            ret.Add(new ControlInputsPanel(vmdata, shipvm));
            ret.Add(new TrollingPanel(vmdata, shipvm));
            ret.Add(new ControlUnitFaultsPanel(vmdata, shipvm));
            ret.Add(new TimeoutFaultsPanel(vmdata, shipvm));
            ret.Add(new DockModeFaultPanel(vmdata, shipvm));
            ret.Add(new SafetyInterlocksPanel(vmdata, shipvm));
            ret.Add(new VersionsPanel(vmdata, shipvm));
            ret.Add(new ControlUnitOutputsPanel(vmdata, shipvm));
            ret.Add(new VMUPanel(vmdata, shipvm));
            ret.Add(new PDMInputsPanel(vmdata, shipvm));
            ret.Add(new PDMOutputsPanel(vmdata, shipvm));
            ret.Add(new VariableList(vmdata));

            return (ret);
        }

        protected void InitEvents()
        {
            AddEventHandler(new StandardMessageEvent(this));
            AddEventHandler(new StandardOnlineOfflineEvent(this));
            AddEventHandler(new CCMFaultEvent(this));
            AddEventHandler(new CCMVariableChangeEvent(this));
            AddEventHandler(new CCMTripTopSpeedEvent(this));
            //AddEventHandler(new CCMRPMMismatchEvent(this)); // Disabled for now
        }

        public CCM()
        {
            ControlUnits.Add(new MRADSControlUnit("Drive Control Unit", 0x29, null, 2));
            ControlUnits.Add(new MRADSControlUnit("Tab Control Unit", 0x28, null, 2));

            ClutchPanels.Add(new MRADSClutchPanel("Clutch Panel", 0x34, null, 2));
        }

        protected override void InitShip()
        {
            InitVMU("VMU Hull Console", 0x01, 1);
            InitVMU("VMU ICE Console", 0x00, 1);

            InitEngine("Port Engine", 0x00, 3);
            InitEngine("Starboard Engine", 0x01, 3);

            InitEngineController("Port Engine Controller", 0x00, 3);
            InitEngineController("Starboard Engine Controller", 0x01, 3);

            InitSKIM("Panel Interface Module", 0x80, 2);

            InitMurphyLCD("Murphy LCD", 0x50, 1);

            InitPDMs(4);

            InitEvents();
        }

        protected void InitPDMs(int channel)
        {
            PDM pdm;

            pdm = new PDM("PDM 24V 1A", 0x01, channel);

            // Inputs
            pdm.AddInput(0, "Engine Compt Flood Level Switch");
            pdm.AddInput(1, "Port Engine Running");
            pdm.AddInput(2, "Port Frame 9 Bilge Float Switch");
            pdm.AddInput(3, "Port Engine Compt Bilge Float Switch");
            pdm.AddInput(8, "Sea Water Pressure Element, Port");
            pdm.AddInput(9, "Differential Pressure (Vaccum), Port");
            pdm.AddInput(10, "Port Frame 9 Bilge Pump Run Signal");
            pdm.AddInput(11, "Port Engine Compt Bilge Pump Run Signal");

            // Outputs
            pdm.AddOutput(0, "Bilge Pump, Frame 9 Port");
            pdm.AddOutput(1, "Bilge Pump, Eng. Compt. Port");
            pdm.AddOutput(2, "Bilge Blower Port");
            pdm.AddOutput(3, "Elex Cooling AC Pump");
            pdm.AddOutput(6, "MTU CCU Port Primary");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 1B", 0x02, channel);

            // Inputs
            pdm.AddInput(10, "Port Seachest Priming Pump Run Signal");
            pdm.AddInput(7, "Port Engine Fuel Prime Pump Enable");
            pdm.AddInput(5, "Hydraulic Oil Filter Clogged, Port Drives System");
            pdm.AddInput(8, "Hydraulic System Pressure, Tabs");
            pdm.AddInput(4, "Hydraulic Oil Filter Clogged, Port Tabs System");
            pdm.AddInput(9, "Port Frame 12 Bilge Pump Run Signal");
            pdm.AddInput(1, "Tabs Hydraulic System Low Pressure Switch");
            pdm.AddInput(3, "Tabs Hydraulic Oil Reservoir Low Level");
            pdm.AddInput(6, "Port Frame 12 Bilge Float Switch");
            pdm.AddInput(2, "Tabs Hydraulic Oil High Temperature Switch");

            // Outputs
            pdm.AddOutput(6, "MTU CCU Stbd Secondary (Bkp)");
            pdm.AddOutput(0, "Ventilation Fan, Port");
            pdm.AddOutput(1, "Port Engine, Fuel Pump");
            pdm.AddOutput(7, "Port Seachest Priming Pump");
            pdm.AddOutput(4, "Bilge Pump, Frame 12 Port");
            pdm.AddOutput(3, "Battery Parallel Solenoid Relay, Port");
            pdm.AddOutput(2, "Crew Compt Lights, Port, Red/Green");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 4A", 0x09, channel);

            // Inputs
            pdm.AddInput(9, "Differential Pressure (Vacuum), Stbd");
            pdm.AddInput(6, "Drives Hydraulic Oil Reservoir Low Level Switch");
            pdm.AddInput(4, "Hydraulic Oil Filter Clogged, Stbd Tabs System");
            pdm.AddInput(7, "Hydraulic Oil Filter Clogged, Stbd Drives System");
            pdm.AddInput(10, "Stbd Frame 9 Bilge Pump Run Signal");
            pdm.AddInput(3, "Drives Hydraulic System Low Pressure Switch");
            pdm.AddInput(8, "Seawater Pressure Element, Stbd");
            pdm.AddInput(0, "Bilge High Level, Frame 14");
            pdm.AddInput(11, "Stbd Engine Compt Bilge Pump Run Signal");
            pdm.AddInput(2, "Engine Compt. Fire Suppression System Activated");
            pdm.AddInput(5, "Drives Hydraulic Oil High Temperature Switch");
            pdm.AddInput(1, "Stbd Engine Running");

            // Outputs
            pdm.AddOutput(0, "Bilge Pump, Frame 9 Stbd");
            pdm.AddOutput(1, "Bilge Pump, Engine Compt Stbd");
            pdm.AddOutput(6, "MTU CCU Stbd Secondary");
            pdm.AddOutput(3, "Engine Compt. Lights, Red/Green");
            pdm.AddOutput(2, "Bilge Blower, Stbd");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 4B", 0x0a, channel);

            // Inputs
            pdm.AddInput(10, "Stbd Seachest Priming Pump Run");
            pdm.AddInput(7, "Stbd Engine Fuel Prime Pump Enable");
            pdm.AddInput(4, "Stbd Engine Compt Bilge Float Switch");
            pdm.AddInput(8, "Hydraulic System Pressure, Drives");
            pdm.AddInput(3, "Stbd Frame 9 Bilge Float Switch");
            pdm.AddInput(9, "Stbd Frame 12 Bilge Pump Run Signal");
            pdm.AddInput(0, "Genset Running");
            pdm.AddInput(2, "Engine Comp R/G Lights Control");
            pdm.AddInput(5, "Stbd Frame 12 Bilge Float Switch");
            pdm.AddInput(1, "Genset Alarm");

            // Outputs
            pdm.AddOutput(6, "MTU CCU Port Secondary (Bkp)");
            pdm.AddOutput(0, "Ventilation Fan, Stbd");
            pdm.AddOutput(1, "Stbd Engine, Fuel Pump");
            pdm.AddOutput(7, "Stbd Seachest Priming Pump");
            pdm.AddOutput(4, "Bilge Pump, Frame 12 Stbd");
            pdm.AddOutput(3, "Battery Parallel Relay, Stbd");
            pdm.AddOutput(2, "Crew Compt. Lights Red/Green");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 2A", 0x03, channel);

            // Inputs
            pdm.AddInput(9, "Port Storage Compt Bilge Float Switch (Convert)");
            pdm.AddInput(6, "Alarm Silence Control");
            pdm.AddInput(4, "Engine Comp Vent Fans Control");
            pdm.AddInput(7, "Common Breaker Reset");
            pdm.AddInput(3, "Ballast Tank Dewater Pump Control");
            pdm.AddInput(8, "Port Storage Compt Bilge Pump Run Signal");
            pdm.AddInput(0, "Ballast Valve Control");
            pdm.AddInput(2, "Ballast Pump Control (Deballast)");
            pdm.AddInput(5, "Bilge Blowers Control");
            pdm.AddInput(1, "Ballast Pump Control (Ballast)");

            // Outputs
            pdm.AddOutput(2, "Bilge Pump, Storage Comp Port");
            pdm.AddOutput(3, "Storage Comp Lights Red/Green");
            pdm.AddOutput(7, "Elex Cooling AC Compressor #2");
            pdm.AddOutput(6, "Elex Cooling AC Compressor #1");
            pdm.AddOutput(4, "Bilge Power, Storage Comp");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 3A", 0x06, channel);

            // Inputs
            pdm.AddInput(6, "Ballast Tank Level 1");
            pdm.AddInput(4, "Ballast Tank Level 3");
            pdm.AddInput(8, "Stbd Storage Compt Bilge Pump Run Signal");
            pdm.AddInput(3, "Ballast Tank Level 4");
            pdm.AddInput(0, "Storage Comp Flood Level");
            pdm.AddInput(2, "Stbd Storage Compt Bilge Float Switch");
            pdm.AddInput(5, "Ballast Tank Level 2");
            pdm.AddInput(1, "Ballast Tank High Level");

            // Outputs
            pdm.AddOutput(6, "Window Defogger Fancoil");
            pdm.AddOutput(0, "Bilge Pump, Storage Comp Stbd");
            pdm.AddOutput(2, "MBMMR AN/PRC-117G 1/1");
            pdm.AddOutput(5, "MBMMR AN/PRC-117G 2/2");
            pdm.AddOutput(4, "MBMMR AN/PRC-117G 2/1");
            pdm.AddOutput(3, "MBMMR AN/PRC-117G 1/2");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 5A", 0x0b, channel);

            // Inputs
            pdm.AddInput(8, "Ballast Dewatering Pump Run Signal");
            pdm.AddInput(5, "Ballast Valve Open");
            pdm.AddInput(3, "Elex Cooling Compressor Binary Switch");
            pdm.AddInput(0, "Elex Cooling Fancoil Freeze Sensor");
            pdm.AddInput(2, "Radio Cabinet Thermoswitch");
            pdm.AddInput(6, "Ballast Valve Closed");
            pdm.AddInput(1, "Elex Cabinet Thermoswitch");

            // Outputs
            pdm.AddOutput(0, "IFF Transponder AN/APX-123");
            pdm.AddOutput(2, "Bilge Pump, Ballast Dewatering");
            pdm.AddOutput(5, "MBMMR AN/PRC-117G 3/2");
            pdm.AddOutput(4, "Stbd Circulation Fans");
            pdm.AddOutput(3, "MBMMR AN/PRC-117G 3/1");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 5B", 0x0c, channel);

            // Inputs
            pdm.AddInput(6, "TCS Control Master (KITS or SSN8 TCS)");
            pdm.AddInput(4, "Deck Lights Control");
            pdm.AddInput(3, "Overhead Strobe Light Control");
            pdm.AddInput(0, "Navigator Lights Control - Running");
            pdm.AddInput(2, "Follow-me Light Control");
            pdm.AddInput(5, "Darken Ship Control");
            pdm.AddInput(1, " Navigation Lights Control - Anchor");

            // Outputs
            pdm.AddOutput(5, "MBMMR AN/PRC-117G 4/2");
            pdm.AddOutput(0, "HF Xceiver AN/PRC-150");
            pdm.AddOutput(1, "MBMMR AN/PRC-117G 4/1");
            pdm.AddOutput(6, "Elex Cooling Fancoil");
            pdm.AddOutput(4, "AN/VIC3 MCS");
            pdm.AddOutput(3, "HUB No. 3 (COMMS)");
            pdm.AddOutput(2, "COMMS Display");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 2B", 0x04, channel);

            // Inputs
            pdm.AddInput(6, "Cockpit Defog System");
            pdm.AddInput(4, "Auxiliary Systems Control Master");
            pdm.AddInput(7, "Elex Cooling");
            pdm.AddInput(3, "Propulsion Control Master");
            pdm.AddInput(8, "Frame 14 Bilge Pump Run Signal");
            pdm.AddInput(0, "COMMS Control Master");
            pdm.AddInput(2, "NAV and CCFLIR Control Master");
            pdm.AddInput(5, "Bilge System Master");
            pdm.AddInput(1, "Weapons System Master");

            // Outputs
            pdm.AddOutput(0, "ENGR Display");
            pdm.AddOutput(1, "HUB No. 2");
            pdm.AddOutput(6, "CCFLIR");
            pdm.AddOutput(4, "Port Circulation Fans");
            pdm.AddOutput(2, "Bilge Pump, Frame 14");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 2C", 0x05, channel);

            // Inputs
            pdm.AddInput(11, "Stbd Engine Compt Bilge Pump Run Switch");
            pdm.AddInput(8, "Bustle Dewater Pump Port Run Switch");
            pdm.AddInput(6, "Bustle Dewater Pump Port Auto Switch");
            pdm.AddInput(9, "Bustle Dewater Pump Stbd Run Switch");
            pdm.AddInput(5, "Stbd Seachest Priming Pump Control");
            pdm.AddInput(10, "Port Engine Compt Bilge Pump Run Switch");
            pdm.AddInput(0, "Battery Parallel");
            pdm.AddInput(4, "Port Seachest Priming Pump Control");
            pdm.AddInput(7, "Bustle  Dewater Pump Stbd Auto Switch");
            pdm.AddInput(1, "Indicator Light Test");

            // Outputs
            pdm.AddOutput(0, "Helm Display");
            pdm.AddOutput(1, "Scanner Power");
            pdm.AddOutput(6, "Furuno MFDBB Processor");
            pdm.AddOutput(3, "ENTR Receiver");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 3B", 0x07, channel);

            // Inputs
            pdm.AddInput(10, "Port Frame 9 Bilge Pump Run Switch");
            pdm.AddInput(6, "Console Thermoswitch 1");
            pdm.AddInput(4, "Port Storage Compt Bilge Pump Auto Switch");
            pdm.AddInput(8, "Port Storage Compt Bilge Pump Run Switch");
            pdm.AddInput(11, "Stbd Frame 9 Bilge Pump Run Switch");
            pdm.AddInput(3, "Stbd Emergency Dewatering Control");
            pdm.AddInput(9, "Stbd Storage Compt Bilge Pump Run Switch");
            pdm.AddInput(0, "Compass Lighting Control");
            pdm.AddInput(2, "Port Emergency Dewatering Control");
            pdm.AddInput(5, "Stbd Storage Compt Bilge Pump Auto Switch");
            pdm.AddInput(1, "Console Thermoswitch 2");

            // Outputs
            pdm.AddOutput(5, "Ethernet Switch, Primary");
            pdm.AddOutput(0, "NAV Display");
            pdm.AddOutput(1, "Axis Video Server");
            pdm.AddOutput(6, "Rugged Power Unit #2");
            pdm.AddOutput(4, "Compass Lighting");
            pdm.AddOutput(3, "NDC-ASW NMEA Mux");
            pdm.AddOutput(2, "NDC-4 NMEA Mux");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 3C", 0x08, channel);

            // Inputs
            pdm.AddInput(9, "Stbd Frame 12 Bilge Pump Run Switch");
            pdm.AddInput(6, "Frame 14 Bilge Pump Auto Switch");
            pdm.AddInput(4, "Port Frame 12 Bilge Pump Auto Switch");
            pdm.AddInput(7, "Ballast Dewater Pump Auto Switch");
            pdm.AddInput(10, "Frame 14 Bilge Pump Run Switch");
            pdm.AddInput(3, "Stbd Frame 9 Bilge Pump Auto Switch");
            pdm.AddInput(8, "Port Frame 12 Bilge Pump Run Switch");
            pdm.AddInput(0, "Port Engine Compt Bilge Pump Auto Switch");
            pdm.AddInput(11, "Ballast Dewater Bilge Pump Run Switch");
            pdm.AddInput(2, "Port Frame 9 Bilge Pump Auto Switch");
            pdm.AddInput(5, "Stbd Frame 12 Bilge Pump Auto Switch");
            pdm.AddInput(1, "Stbd Engine Compt Bilge Pump Auto Switch");

            // Outputs
            pdm.AddOutput(5, "FLIR DBA");
            pdm.AddOutput(0, "CAN Bus Reader 1");
            pdm.AddOutput(1, "CAN Bus Reader 2");
            pdm.AddOutput(6, "Rugger Power Unit #1");
            pdm.AddOutput(4, "HUB No. 1");
            pdm.AddOutput(3, "Ethernet Switch Seconday");
            pdm.AddOutput(2, "CAN Bus Reader 3");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 6A", 0x0d, channel);

            // Inputs
            pdm.AddInput(2, "Vector Drives Controller Summary Alarm");
            pdm.AddInput(3, "Vector Tabs Controller Summary Alarm");

            // Outputs
            pdm.AddOutput(5, "Vector Stbd Engine Controller");
            pdm.AddOutput(0, "Vector Drive Backup, Port");
            pdm.AddOutput(1, "Vector Drive Backup, Stbd");
            pdm.AddOutput(6, "Vector Hyd Dump Valve Controller");
            pdm.AddOutput(4, "Vector Port Engine Controller");
            pdm.AddOutput(3, "Vector Tabs Backup");
            pdm.AddOutput(2, "Vector Clutch Panel Backup");
            pdm.AddOutput(7, "Vector Clutch Panel Auto");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 12V 1A", 0x0e, channel);

            // Outputs
            pdm.AddOutput(0, "NMEA 2000 Network Power");
            pdm.AddOutput(1, "Receptacle, 12V");
            pdm.AddOutput(2, "Receptacles, 12V");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 12V 1B", 0x0f, channel);

            // Outputs
            pdm.AddOutput(6, "Receptacle 12V4");
            pdm.AddOutput(0, "Marine Band Radio");
            pdm.AddOutput(1, "Receptacle 12V1");
            pdm.AddOutput(5, "Searchlight Charger");
            pdm.AddOutput(2, "Receptacle 12V2");
            pdm.AddOutput(3, "Receptacle 12V3");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 12V 1C", 0x10, channel);

            // Outputs
            pdm.AddOutput(0, "Receptacle, 12V");
            pdm.AddOutput(1, "Receptacles, 12V");
            pdm.AddOutput(2, "Generator Seawater Valve Power");

            PDMs.Add(pdm);

            foreach (var p in PDMs)
                p.AddVariables(Config);
        }

        protected void InitDriveCU(MRADSControlUnit CU)
        {
            PGNDecoder decoder;

            decoder = CU.AddPGN(0xff30);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("StbdActualShaftRPM", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("PortActualShaftRPM", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("IntegratedStbdClutchAheadCommand", 4, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("IntegratedPortClutchAheadCommand", 4, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("IntegratedStbdClutchReverseCommand", 4, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("IntegratedPortClutchReverseCommand", 4, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("DockTransitJoystickSwitch", 5, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("JoystickTillerStationSwitch", 5, 1, true));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("ThrottleHelmStationSwitch", 5, 2, true));

            decoder = CU.AddPGN(0xab0a);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("DesiredPortTrollingDutyCycle", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("DesiredPortTrollingShaftRPM", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MaxPortTrollingDutyCycleAdjustment", 4, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("PortTrollingKI", 6, 7));

            decoder = CU.AddPGN(0xab0b);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("DesiredStbdTrollingDutyCycle", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("DesiredStbdTrollingShaftRPM", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MaxStbdTrollingDutyCycleAdjustment", 4, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("StbdTrollingKI", 6, 7));

            decoder = CU.AddPGN(0xab0c);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MaxPortTrollingDutyCycle", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MaxStbdTrollingDutyCycle", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("TiebarAnalogInput", 4, 5, StandardAInScale));

            decoder = CU.AddPGN(0xff88);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("ScaledPortDriveTrimFeedback", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("ScaledPortDriveSteerFeedback", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("ScaledStbdDriveTrimFeedback", 4, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("ScaledStbdDriveSteerFeedback", 6, 7));

            decoder = CU.AddPGN(0xff89);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("TillerFault", 0, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("JoyXFault", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("JoyYFault", 0, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("NavigatorPortTrimJoystickFault", 0, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("NavigatorStbdTrimJoystickFault", 0, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortLeverFault", 1, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdLeverFault", 1, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("FwdPanelPortTrimJoystickFault", 1, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("FwdPanelStbdTrimJoystickFault", 1, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("HelmFault", 1, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("JoystickDockingzone", 2, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdTrimSensorFault", 3, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortTrimSensorFault", 3, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdSteerSensorFault", 3, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortSteerSensorFault", 3, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdTrimNFUFault", 4, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortTrimNFUFault", 4, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdSteerNFUFault", 4, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortSteerNFUFault", 4, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("CalibrationFault", 5, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("GeneralAlarm", 5, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("CANFault", 5, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("DockmodeFault", 5, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("ClutchCommunicationTimeout", 5, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdEngineCommunicationTimeout", 6, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortEngineCommunicationTimeout", 6, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdEngineControllerCommunicationTimeout", 6, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortEngineControllerCommunicationTimeout", 6, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("MurphyLCDCommunicationTimeout", 6, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("DockingTransitState", 7, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("AutoTrimSwitch", 7, 1));

            decoder = CU.AddPGN(0xff8a);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("ScaledPortEngineCommand", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("ScaledStbdEngineCommand", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("ScaledActivePortThrottleControlInputCommand", 4, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("ScaledActiveStbdThrottleControlInputCommand", 6, 7));

            decoder = CU.AddPGN(0xff8d);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("HighPortRPMClutchInterlock", 0, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("HighStbdRPMClutchInterlock", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("DisengagedPortClutchInterlock", 0, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("DisengagedStbdClutchInterlock", 0, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("DockmodeFaultDueToSensorFault", 1, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("DockmodeFaultDueToOFFTrollingSwitch", 1, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("DockmodeFaultDueToLowRPM", 1, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("DockmodeFaultDueToJoyXFault", 1, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("DockmodeFaultDueToEngineCommunication", 1, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("DockmodeFaultDueToEngineControllerCommunication", 1, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("DockmodeFaultDueToClutchCommunication", 1, 6));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("DockmodeFaultDueToTillerFault", 1, 7));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("SWRevision", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MajorSWVersion", 4, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MinorSWVersion", 5, 6));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("StationInControl", 7, 7));

            decoder = CU.AddPGN(0xffb1);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("PortEngineAnalogOutput", 0, 1, StandardAOutScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("StbdEngineAnalogOutput", 2, 3, StandardAOutScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("JoystickXAnalogInput", 4, 5, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("JoystickYAnalogInput", 6, 7, StandardAInScale));

            decoder = CU.AddPGN(0xffb2);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("TillerAnalogInput", 0, 1, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("NavigatorStationPortTrimAnalogInput", 2, 3, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("NavigatorStationStbdTrimAnalogInput", 4, 5, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("PortThrottleAnalogInput", 6, 7, StandardAInScale));

            decoder = CU.AddPGN(0xffb3);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("StbdThrottleAnalogInput", 0, 1, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("HelmAnalogInput", 2, 3, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("FwdPanelPortTrimAnalogInput", 4, 5, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("FwdPanelStbdTrimAnalogInput", 6, 7, StandardAInScale));

            decoder = CU.AddPGN(0xffb4);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("PortSteerFeedbackAnalogInput", 0, 1, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("StbdSteerFeedbackAnalogInput", 2, 3, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("PortTrimFeedbackAnalogInput", 4, 5, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("StbdTrimFeedbackAnalogInput", 6, 7, StandardAInScale));

            decoder = CU.AddPGN(0xffb5);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("PortSteerAnalogOutput", 0, 1, StandardAOutScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("StbdSteerAnalogOutput", 2, 3, StandardAOutScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("PortTrimAnalogOutput", 4, 5, StandardAOutScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("StbdTrimAnalogOutput", 6, 7, StandardAOutScale));
        }

        protected void InitTabCU(MRADSControlUnit CU)
        {
            PGNDecoder decoder;

            decoder = CU.AddPGN(0xff8c);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("ScaledPortTabFeedback", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("ScaledStbdTabFeedback", 4, 5));

            decoder = CU.AddPGN(0xff87);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("NavigatorStationRollJoystickFault", 0, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("NavigatorStationTrimJoystickFault", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("FwdPanelRollJoystickFault", 0, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("FwdPanelTrimJoystickFault", 0, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("TillerFault", 0, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("HelmFault", 0, 5));

            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdTabSensorFault", 3, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortTabSensorFault", 3, 2));

            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdTabNFUFault", 4, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortTabNFUFault", 4, 2));

            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("CalibrationFault", 5, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("GeneralAlarm", 5, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("CANFault", 5, 2));

            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("DriveControlUnitCommunicationTimeout", 6, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("ClutchCommunicationTimeout", 6, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortEngineCommunicationTimeout", 6, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdEngineCommunicationTimeout", 6, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortEngineControllerCommunicationTimeout", 6, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdEngineControllerCommunicationTimeout", 6, 5));

            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("IntegratedSteeringActive", 7, 0));

            decoder = CU.AddPGN(0xff8e);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("IntegratedSteeringSwitch", 1, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("SWRevision", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MajorSWVersion", 4, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MinorSWVersion", 5, 6));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("StationInControl", 7, 7));

            decoder = CU.AddPGN(0xffc1);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("TillerAnalogInput", 0, 1, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("HelmAnalogInput", 2, 3, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("NavigatorRollJoystickAnalogInput", 4, 5, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("NavigatorTrimJoystickAnalogInput", 6, 7, StandardAInScale));

            decoder = CU.AddPGN(0xffc2);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("FwdPanelRollJoystickAnalogInput", 0, 1, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("FwdPanelTrimJoystickAnalogInput", 2, 3, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("PortTabAnalogInput", 4, 5, StandardAInScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("StbdTabAnalogInput", 6, 7, StandardAInScale));

            decoder = CU.AddPGN(0xffc3);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("PortTabAnalogOutput", 0, 1, StandardAOutScale));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("StbdTabAnalogOutput", 2, 3, StandardAOutScale));
        }

        void InitVMU(string name, byte source, int channel)
        {
            MRADSDataProvider vmu = Config.CreateProvider(name, source, channel);

            var decoder = vmu.AddPGN(0xffb0);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("Roll", d => DecodeVMUValue(0, d)));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("Pitch", d => DecodeVMUValue(1, d)));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("Yaw", d => DecodeVMUValue(2, d)));
        }

        double DecodeVMUValue(int index, byte[] data)
        {
            double ret = data[index * 2];
            byte f = data[index * 2 + 1];

            if (f != 0)
                ret += f / Math.Pow(10, Math.Ceiling(Math.Log10(f)));

            if ((data[6] & (1 << index)) != 0)
                ret = -ret;

            return (ret);
        }

        protected void InitEngine(string name, byte source, int channel)
        {
            MRADSDataProvider engine = Config.CreateProvider(name, source, channel);
            PGNDecoder decoder;

            decoder = engine.AddPGN(0xf005);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("CurrentGearAHD", d => d[3] == 126 || d[3] == 127));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("CurrentGearREV", d => d[3] == 124));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("CurrentGearNEU", d => d[3] != 124 && d[3] != 126 && d[3] != 127));

            decoder = engine.AddPGN(0xfef8);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("ClutchPressure", d => d[0] * 16 * 0.145038));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("TransmissionOilTemperature", d => (((d[5] << 8) | (d[4])) * 0.03125) - 273));

            decoder = engine.AddPGN(0xf004);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("EngineRPM", d => (d[4] << 5) | (d[3] >> 3)));

            decoder = engine.AddPGN(0xfeef);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("EngineOilPressure", d => d[3]* 4 * 0.145038));

            decoder = engine.AddPGN(0xfee5);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("TotalEngineHours", d => (int)(BitConverter.ToUInt32(d, 0) * 0.05)));

            decoder = engine.AddPGN(0xfef7);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("BatteryVoltage", d => BitConverter.ToUInt16(d, 6) * 0.05));

            decoder = engine.AddPGN(0xf003);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("EngineLoad", 2, 2));

            decoder = engine.AddPGN(0xfedf);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("DesiredRPM", d => (d[2] << 5) | (d[1] >> 3)));
        }

        protected void InitEngineController(string name, byte source, int channel)
        {
            MRADSDataProvider enginectrl = Config.CreateProvider(name, source, channel);
            PGNDecoder decoder;

            decoder = enginectrl.AddPGN(0xab00);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("ActualShaftRPM", 0, 1));
        }

        protected void InitSKIM(string name, byte source, int channel)
        {
            MRADSDataProvider skim = Config.CreateProvider(name, source, channel);
            PGNDecoder decoder;
            int threshold = 100;

            decoder = skim.AddPGN(0xff21);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("TabAutoBackupSwitch", d => d[0] < threshold));

            decoder = skim.AddPGN(0xff23);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortSteerAutoBackupSwitch", d => d[0] < threshold));

            decoder = skim.AddPGN(0xff24);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortDriveAutoBackupSwitch", d => d[0] < threshold));

            decoder = skim.AddPGN(0xff25);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortEngineAutoBackupSwitch", d => d[0] < threshold));

            decoder = skim.AddPGN(0xff26);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdSteerAutoBackupSwitch", d => d[0] < threshold));

            decoder = skim.AddPGN(0xff27);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdDriveAutoBackupSwitch", d => d[0] < threshold));

            decoder = skim.AddPGN(0xff28);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdEngineAutoBackupSwitch", d => d[0] < threshold));
        }

        protected void InitMurphyLCD(string name, byte source, int channel)
        {
            MRADSDataProvider skim = Config.CreateProvider(name, source, channel);
            PGNDecoder decoder;

            decoder = skim.AddPGN(0xff8f);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("BoatSpeed", d => BitConverter.ToUInt16(d, 0) / 100.0));
        }

        protected override bool CheckShipID(MRADSShip ship)
        {
            var v = ship.State.GetValue(ship.ControlUnits[0], "MajorSWVersion");
            if (!v.Set)
                return false;

            var vmu = ship.State.ProviderOnline(ship.Config.GetProvider("VMU Hull Console")) || ship.State.ProviderOnline(ship.Config.GetProvider("VMU ICE Console"));

            return v.GetValue<int>() == 81 && vmu;
        }

        protected override DefaultBindVM GetViewModel(MRADSDataVM vmship, MRADSShipConfig config) => new CCMVM(vmship, config);

        protected override void InitClutchPanel(MRADSClutchPanel CP)
        {
            PGNDecoder decoder;

            decoder = CP.AddPGN(0xff31);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("TrollingONOFFSwitch", 0, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("BackupAutoSwitch", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("IntegratedManualSwitch", 0, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("DriveControlUnitCommunicationTimeout", 0, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdRPMStall", 0, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortRPMStall", 0, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("InvalidClutchCommandAtStartup", 0, 6));

            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MajorSWVersion", 1, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MinorSWVersion", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("SWRevision", 4, 5));

            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdEngageAHDCmd", 6, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortEngageAHDCmd", 6, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdDisengageCmd", 6, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortDisengageCmd", 6, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdEngageREVCmd", 6, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortEngageREVCmd", 6, 5));

            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdEngageAHDFeedback", 7, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortEngageAHDFeedback", 7, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdDisengageFeedback", 7, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortDisengageFeedback", 7, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StbdEngageREVFeedback", 7, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortEngageREVFeedback", 7, 5));
        }

        public class PDM
        {
            public class Input
            {
                public int Bit;
                public string VarName;
            }

            public class Output
            {
                public int Index;
                public string VarName;
            }

            public List<Input> Inputs { get; } = new List<Input>();
            public List<Output> Outputs { get; } = new List<Output>();

            public MRADSDataProvider Provider { get; private set; }

            public string Name { get; }
            byte source;
            int channel;

            public PDM(string name, byte source, int channel)
            {
                Name = name;
                this.source = source;
                this.channel = channel;
            }

            public void AddInput(int bit, string varname)
            {
                Inputs.Add(new Input() { Bit = bit, VarName = varname });
            }

            public void AddOutput(int index, string varname)
            {
                Outputs.Add(new Output() { Index = index, VarName = varname });
            }

            public void AddVariables(MRADSShipConfig config)
            {
                Provider = config.CreateProvider(Name, source, channel);
                PGNDecoder decoder;

                decoder = Provider.AddPGN(0xff01);

                foreach (var input in Inputs)
                    decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(input.VarName, input.Bit / 8, input.Bit % 8));

                decoder = Provider.AddPGN(0xff07);

                foreach (var output in Outputs.Where(a => a.Index < 4))
                    decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat(output.VarName, d => PDMIntToFloat(d, output.Index * 2)));

                decoder = Provider.AddPGN(0xff08);

                foreach (var output in Outputs.Where(a => a.Index >= 4))
                    decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat(output.VarName, d => PDMIntToFloat(d, output.Index * 2 - 8)));
            }

            double PDMIntToFloat(byte[] data, int index)
            {
                int v = (data[index] << 8) | data[index + 1];

                return (v / 100.0);
            }
        }

        protected override void InitControlUnit(MRADSControlUnit CU)
        {
            if (CU.Name == "Drive Control Unit")
                InitDriveCU(CU);
            else if (CU.Name == "Tab Control Unit")
                InitTabCU(CU);
        }
    }
}

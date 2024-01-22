using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRADS2.Panels;
using MRADS2.Panels.StandardShip;
using MRADS2.Ships.ViewModel;

namespace MRADS2.Ships.StandardShip
{
    class StandardShip : MRADSShip
    {
        public StandardShip()
        {
            ControlUnits.Add(new MRADSControlUnit("Port Control Unit", 0x00, new string[] { "PortOutboard", "PortInboard" }, 2, 4));
            ControlUnits.Add(new MRADSControlUnit("Starboard Control Unit", 0x01, new string[] { "StarboardInboard", "StarboardOutboard" }, 3, 4));

            ClutchPanels.Add(new MRADSClutchPanel("Port Clutch Panel", 0x34, new string[] { "PortOutboard", "PortInboard" }, 2));
            ClutchPanels.Add(new MRADSClutchPanel("Starboard Clutch Panel", 0x34, new string[] { "StarboardInboard", "StarboardOutboard" }, 3));

            Config.SetAttribute("WaterJetsPerSide", 2);
            Config.SetAttribute("EnginesPerSide", 2);
        }

        protected override void InitControlUnit(MRADSControlUnit CU)
        {
            PGNDecoder decoder;
            var prefixes = (string[])CU.Data;
            string prefix1 = prefixes[0], prefix2 = prefixes[1];

            decoder = CU.AddPGN(0xff71);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Station1ThrottleAnalogInput", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Station2ThrottleAnalogInput", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Station3ThrottleAnalogInput", 4, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("RemoteThrottleAnalogInput", 6, 7));

            decoder = CU.AddPGN(0xff72);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Station1AzimuthAnalogInput", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Station2AzimuthAnalogInput", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Station3AzimuthAnalogInput", 4, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("RemoteAzimuthAnalogInput", 6, 7));

            decoder = CU.AddPGN(0xff73);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix1 + "NozzleFeedbackAnalogInput", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix2 + "NozzleFeedbackAnalogInput", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix1 + "BucketFeedbackAnalogInput", 4, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix2 + "BucketFeedbackAnalogInput", 6, 7));

            decoder = CU.AddPGN(0xff74);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("CombinedSeparateAnalogInput", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("PortOrStbdLeverAnalogInput", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("OnOffCombinedModeState", 4, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("PortOrStbdLeverSwitchState", 4, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("OnOffRemoteLeverModeState", 4, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("BackupAutoStbdOutboardJet1State", 5, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("BackupAutoPortOutboardJet2State", 5, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("BackupAutoStbdInboardJet3State", 5, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("BackupAutoPortInboardJet4State", 5, 3));

            var pdecoder = CU.AddParameterPGN(0xff75);
            foreach (var p in ParameterList)
                pdecoder.SetParameter(p.index, p.name);

            decoder = CU.AddPGN(0xff76);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix1 + "NozzleValveOutput", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix2 + "NozzleValveOutput", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix1 + "BucketValveOutput", 4, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix2 + "BucketValveOutput", 6, 7));

            decoder = CU.AddPGN(0xff77);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix1 + "UnscaledNozzleCommand", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix1 + "UnscaledNozzleFeedback", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix2 + "UnscaledNozzleCommand", 4, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix2 + "UnscaledNozzleFeedback", 6, 7));

            decoder = CU.AddPGN(0xff78);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix1 + "UnscaledBucketCommand", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix1 + "UnscaledBucketFeedback", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix2 + "UnscaledBucketCommand", 4, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix2 + "UnscaledBucketFeedback", 6, 7));

            decoder = CU.AddPGN(0xff30);
            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("ClutchPermissivesState", 0, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix2 + "ClutchPermissiveState", 0, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix1 + "ClutchPermissiveState", 0, 1));

            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MajorSWVersion", 2, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MinorSWVersion", 3, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("SWRevision", 5, 6));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MessageCounter", 7, 7));

            decoder = CU.AddPGN(0xff10);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix1 + "ScaledBucketFeedback", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix1 + "ScaledNozzleFeedback", 2, 3));

            decoder = CU.AddPGN(0xff11);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix2 + "ScaledBucketFeedback", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix2 + "ScaledNozzleFeedback", 2, 3));

            InitFaults(CU.AddPGN(0xff12), prefix1, prefix2);

            decoder = CU.AddPGN(0xff13);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix1 + "ScaledEngineCommand", 0, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt(prefix2 + "ScaledEngineCommand", 1, 1));

            decoder = CU.AddPGN(0xff15);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("CalibrationFdbkType", 1, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("CalibrationFdbkOperation", 2, 2));

            decoder = CU.AddPGN(0xff16);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("StationTransferPendingTakeControlThrustLever", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("StationTransferPendingTakeControlAzimuthLever", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StationTransferPendingMatchBuckets", 4, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("StationTransferPendingMatchNozzles", 4, 1));

            decoder = CU.AddPGN(0xff60);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("LocalStationTransferPermissive", 4, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("LocalStationTransferUnderFault", 4, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("StationInControl", 5, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("UnitPowerUpComplete", 7, 0));
        }

        void InitFaults(PGNDecoder decoder, string prefix1, string prefix2)
        {
            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Station1CommandFaults", 0, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("Station1ThrustFault", 0, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("Station1AzimuthFault", 0, 1));

            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Station2CommandFaults", 1, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("Station2ThrustFault", 1, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("Station2AzimuthFault", 1, 1));

            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Station3CommandFaults", 2, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("Station3ThrustFault", 2, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("Station3AzimuthFault", 2, 1));

            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("FeedbackSensorFaults", 3, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix2 + "BucketFdbkFault", 3, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix1 + "BucketFdbkFault", 3, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix2 + "NozzleFdbkFault", 3, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix1 + "NozzleFdbkFault", 3, 3));

            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("NonFollowUpFaults", 4, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix2 + "BucketNFUFault", 4, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix1 + "BucketNFUFault", 4, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix2 + "NozzleNFUFault", 4, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix1 + "NozzleNFUFault", 4, 3));

            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("ControllerFaults", 5, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("CalibrationFault", 5, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("GeneralAlarm", 5, 1));

            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("SystemFaults", 6, 6));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("ClutchPanelCommFault", 6, 0));
        }

        protected override void InitClutchPanel(MRADSClutchPanel CP)
        {
            PGNDecoder decoder;
            var prefixes = (string[])CP.Data;
            string prefix1 = prefixes[0], prefix2 = prefixes[1];

            decoder = CP.AddPGN(0xff31);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix2 + "ClutchEngagedFeedbackState", 0, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix1 + "ClutchEngagedFeedbackState", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix2 + "ClutchBackflushFeedbackState", 0, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix1 + "ClutchBackflushFeedbackState", 0, 3));

            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("CANRxFault", 4, 3));

            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MajorSWVersion", 1, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MinorSWVersion", 5, 5));                  
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("SWRevision", d => (d[6] << 8) | d[7]));  

            decoder = CP.AddPGN(0xff33);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix1 + "ClutchPanelEngagedLEDState", 0, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix2 + "ClutchPanelEngagedLEDState", 1, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix1 + "ClutchPanelBackflushLEDState", 2, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool(prefix2 + "ClutchPanelBackflushLEDState", 3, 0));
        }

        protected override void InitShip()
        {
            AdditionalInit();

            InitEvents();
        }

        protected virtual void AdditionalInit()
        {
            InitGPS("GPS", 1);
            InitVMU("VMU", 1);
            InitLCD("LCD", 4);
        }

        protected virtual void InitEvents()
        {
            AddEventHandler(new StandardMessageEvent(this));
            AddEventHandler(new StandardOnlineOfflineEvent(this));
            AddEventHandler(new StandardShipFaultEvent(this));
            AddEventHandler(new StandardShipVariableChangeEvent(this));
            AddEventHandler(new StandardShipCalibrationEvent(this));
        }

        MRADSDataProvider InitGPS(string name, int channel)
        {
            MRADSDataProvider gps = Config.CreateProvider(name, 0x7f, channel);
            PGNDecoder decoder;

            decoder = gps.AddPGN(0xf112);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("Heading", d => BitConverter.ToUInt16(d, 1) * 180 / Math.PI / 10000));

            decoder = gps.AddPGN(0xf801);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("Latitude", d => BitConverter.ToInt32(d, 0) * 1E-7));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("Longitude", d => BitConverter.ToInt32(d, 4) * 1E-7));

            decoder = gps.AddPGN(0xf113);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("RateOfTurn", d => BitConverter.ToInt32(d, 1) * 1E-6 / 32 * 180 / Math.PI));

            decoder = gps.AddPGN(0xf802);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("SpeedOverGround", d => BitConverter.ToUInt16(d, 4) * 0.01 * 1.943844));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("CourseOverGround", d => BitConverter.ToUInt16(d, 2) * 1E-4 * 180 / Math.PI));

            decoder = gps.AddPGN(0xf010);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Year", d => ParseGPSTime(d).Year));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Month", d => ParseGPSTime(d).Month));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Day", d => ParseGPSTime(d).Day));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Hour", d => ParseGPSTime(d).Hour));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Minute", d => ParseGPSTime(d).Minute));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Second", d => ParseGPSTime(d).Second));

            return (gps);
        }

        DateTime ParseGPSTime(byte[] data)
        {
            uint days, ticks;

            days = BitConverter.ToUInt16(data, 2);
            ticks = BitConverter.ToUInt32(data, 4);

            return (DateTime.UnixEpoch.AddDays(days).AddMilliseconds(ticks / 10.0));
        }

        MRADSDataProvider InitVMU(string name, int channel)
        {
            MRADSDataProvider vmu = Config.CreateProvider(name, 0xe2, channel);
            PGNDecoder decoder;

            decoder = vmu.AddPGN(0xf029);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("PitchAngle", d => (ToUInt24(d, 0) - 8192000.0) / 32768));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("RollAngle", d => (ToUInt24(d, 3) - 8192000.0) / 32768));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("AngleLatency", d => d[7] / 2.0));

            decoder = vmu.AddPGN(0xf02a);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("PitchRate", d => (BitConverter.ToUInt16(d, 0) - 32000.0) / 128));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("RollRate", d => (BitConverter.ToUInt16(d, 2) - 32000.0) / 128));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("YawRate", d => (BitConverter.ToUInt16(d, 4) - 32000.0) / 128));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("RateLatency", d => d[7] / 2.0));

            decoder = vmu.AddPGN(0xf02d);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("YAxisAcceleration", d => (BitConverter.ToUInt16(d, 0) - 32000.0) / 100));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("XAxisAcceleration", d => (BitConverter.ToUInt16(d, 2) - 32000.0) / 100));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("ZAxisAcceleration", d => (BitConverter.ToUInt16(d, 4) - 32000.0) / 100));

            return (vmu);
        }

        uint ToUInt24(byte[] b, int index)
        {
            return (b[index] | ((uint)b[index + 1] << 8) | ((uint)b[index + 2] << 16));
        }

        MRADSDataProvider InitLCD(string name, int channel)
        {
            MRADSDataProvider lcd = Config.CreateProvider(name, new byte[] { 0x00, 0x03 }, channel);
            PGNDecoder decoder;

            decoder = lcd.AddPGN(0xfff0);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("BackupAutoStateStarboardOutboard", 0, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("BackupAutoStatePortOutboard", 1, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("BackupAutoStateStarboardInboard", 2, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("BackupAutoStatePortInboard", 3, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("WJInterfaceModuleFault", 4, 0));

            decoder = lcd.AddPGN(0xff14);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("CalibrationCmdDestination", 0, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("CalibrationCmdType", 1, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("CalibrationCmdOperation", 2, 2));

            return (lcd);
        }

        public override IEnumerable<BasePanel> GetPanels(MRADSDataVM vmdata)
        {
            List<BasePanel> ret = new List<BasePanel>();

            var shipvm = (StandardShipVM)ShipViewModel;

            ret.Add(new GPSPanel(vmdata, shipvm.GPS));
            ret.Add(new VMUPanel(vmdata, shipvm.VMU));
            ret.Add(new LCDPanel(vmdata, shipvm));
            ret.Add(new SepCombPanel(vmdata, shipvm));
            ret.Add(new FeedbacksPanel(vmdata, shipvm));
            ret.Add(new ControlInputsPanel(vmdata, shipvm));
            ret.Add(new StationTransferPanel(vmdata, shipvm));
            ret.Add(new BackupControlsPanel(vmdata, shipvm));
            ret.Add(new ControlUnitInfoPanel(vmdata, shipvm));
            ret.Add(new CommunicationTimeoutFaultsPanel(vmdata, shipvm));
            ret.Add(new ControlInputsSensorFaultsPanel(vmdata, shipvm));
            ret.Add(new ClutchPanel(vmdata, shipvm));
            ret.Add(new ActuatorControlPanel(vmdata, shipvm));
            ret.Add(new CalibrationPanel(vmdata, shipvm));
            ret.Add(new ParametersPanel(vmdata, shipvm));
            ret.Add(new VariableList(vmdata));

            return (ret);
        }

        protected override DefaultBindVM GetViewModel(MRADSDataVM vmship, MRADSShipConfig config)
        {
            return (new StandardShipVM(vmship, config));
        }

        protected override bool CheckShipID(MRADSShip ship)
        {
            var v = ship.State.GetValue(ship.ControlUnits[0], "MajorSWVersion");
            if (!v.Set)
                return (false);

            return (v.GetValue<int>() == 39);
        }

        static readonly (int index, string name)[] ParameterList = new (int index, string name)[]
        {
            (5,"NUMBER_WATERJET_UNITS"),
            (6,"NUMBER_BOWTHRUSTER_UNITS"),
            (7,"STA1_CONFIG"),
            (8,"STA2_CONFIG"),
            (9,"ENGINE_INTERFACE"),
            (10,"NOZZLE_LINKAGE"),
            (11,"OPTION_AUTOPILOT"),
            (12,"OPTION_INTSTEER"),
            (13,"INDICATION_CONFIG"),
            (14,"NOZZLEMAP_FLIP"),
            (15,"ERSWITCH_ENABLE"),
            (16,"STA2_DOCKINGMODE_LOCK"),
            (17,"STA3_DOCKINGMODE_LOCK"),
            (18,"OPTION_IDLECONTROL"),
            (19,"OPTION_CLUTCHINDICATION"),
            (20,"THROTTLE_LEVER_DETENT"),
            (21,"STA_1_ZAXIS"),
            (22,"STA_2_ZAXIS"),
            (23,"STA_3_ZAXIS"),
            (24,"STA1_HELMLOCK"),
            (26,"INBDCLTCH_INTERLK"),
            (27,"LCD_COMMS_MODE"),
            (30,"OPTION_ENGINE_3"),
            (31,"BOOSTER_DELAY"),
            (32,"BOOSTER_RAMP"),
            (33,"SJ_CLUTCH"),
            (34,"DK_TR_SWITCH"),
            (35,"VCI_CAN_BUS"),
            (36,"ROLLOVER_SWITCH"),
            (37,"EEPROM_STATUS"),
            (38,"VERSION"),
            (39,"STATUS_AUTOCAL"),
            (40,"KP_UP"),
            (41,"KD"),
            (42,"KI_UP"),
            (43,"KP_DN"),
            (44,"KI_DN"),
            (45,"MAX_HILO_AI"),
            (46,"MAX_HILO_ER"),
            (47,"MAX_POSITION_ERROR"),
            (48,"TOL_AD"),
            (49,"MIN_RANGE_AD"),
            (50,"MAX_RANGE_AD"),
            (51,"DEADZONE_JOYSTICK_X"),
            (52,"DEADZONE_JOYSTICK_Y"),
            (53,"DEADZONE_LEVERS"),
            (54,"DEADZONE_RPM"),
            (55,"DEADZONE_RPM_LEVER"),
            (56,"NOISEBAND"),
            (57,"BKTMOVEMENT_RPMDELAY"),
            (58,"JSXMIN_1"),
            (59,"JSXMID_1"),
            (60,"JSXMAX_1"),
            (61,"JSYMIN_1"),
            (62,"JSYMID_1"),
            (63,"JSYMAX_1"),
            (64,"JSXMIN_2"),
            (65,"JSXMID_2"),
            (66,"JSXMAX_2"),
            (67,"JSYMIN_2"),
            (68,"JSYMID_2"),
            (69,"JSYMAX_2"),
            (70,"JSXMIN_3"),
            (71,"JSXMID_3"),
            (72,"JSXMAX_3"),
            (73,"JSYMIN_3"),
            (74,"JSYMID_3"),
            (75,"JSYMAX_3"),
            (77,"CAN_XMIT_RATE"),
            (78,"HLMMIN_1"),
            (79,"HLMMID_1"),
            (80,"HLMMAX_1"),
            (81,"HLMMIN_2"),
            (82,"HLMMID_2"),
            (83,"HLMMAX_2"),
            (84,"HLMMIN_3"),
            (85,"HLMMID_3"),
            (86,"HLMMAX_3"),
            (87,"DEFAULT_MIN_HELM"),
            (88,"DEFAULT_MAX_HELM"),
            (89,"HELM_MATCH_PARAM"),
            (90,"DELHELM_MIDRANGE_STA1"),
            (91,"DELHELM_MIDRANGE_STA2"),
            (92,"DELHELMCOM_MIDRANGE_STA1"),
            (93,"DELHELMCOM_MIDRANGE_STA2"),
            (94,"THRMIN_1"),
            (95,"THRMID_1"),
            (96,"THRMAX_1"),
            (97,"THRMIN_2"),
            (98,"THRMID_2"),
            (99,"THRMAX_2"),
            (100,"THRMIN_1_STA2"),
            (101,"THRMID_1_STA2"),
            (102,"THRMAX_1_STA2"),
            (103,"THRMIN_2_STA2"),
            (104,"THRMID_2_STA2"),
            (105,"THRMAX_2_STA2"),
            (106,"BKTLEVERMIN_1"),
            (107,"BKTLEVERMID_1"),
            (108,"BKTLEVERMAX_1"),
            (109,"BKTLEVERMIN_2"),
            (110,"BKTLEVERMID_2"),
            (111,"BKTLEVERMAX_2"),
            (112,"DEFAULT_MIN_THROTTLE"),
            (113,"DEFAULT_MAX_THROTTLE"),
            (114,"MID_DA_OUT"),
            (115,"MAX_DA_OUT"),
            (116,"MID_DA_OUT_INT"),
            (117,"MAX_DA_OUT_INT"),
            (118,"SAFETY"),
            (119,"VALVE_STEP"),
            (120,"THRESHOLD_POSITIONERROR"),
            (121,"ERROR_XING"),
            (122,"DEFAULT_NOZZLE_RATIO_DOCKING_AST_OVER_AHD"),
            (123,"DEFAULT_NOZZLE_OFFSET_DOCKING_THRUSTINGAHEAD"),
            (124,"NOZZLE_OFFSET_DOCKING_THRUSTINGAHEAD_MOVEPORT"),
            (125,"NOZZLE_OFFSET_DOCKING_THRUSTINGASTERN_MOVEPORT"),
            (126,"NOZZLE_OFFSET_DOCKING_THRUSTINGAHEAD_MOVESTBD"),
            (127,"NOZZLE_OFFSET_DOCKING_THRUSTINGASTERN_MOVESTBD"),
            (128,"MAX_DOCKING_MOVEPORT_SJ"),
            (129,"MAX_DOCKING_MOVESTBD_SJ"),
            (130,"BUCKET_INVGAIN_Y_HI"),
            (131,"BUCKET_INVGAIN_Y_LO"),
            (132,"BUCKET_INVGAIN_TRANSIT_Y_HI"),
            (133,"BUCKET_INVGAIN_TRANSIT_Y_LO"),
            (134,"BUCKET_INVGAIN_DOCKING_UPWARD"),
            (135,"BUCKET_INVGAIN_DOCKING_DOWNWARD"),
            (136,"SPEED_MAXJX_THRUSTINGAHEAD_MOVINGLEFT"),
            (137,"SPEED_MAXJX_THRUSTINGASTERN_MOVINGLEFT"),
            (138,"SPEED_MAXJX_THRUSTINGAHEAD_MOVINGRIGHT"),
            (139,"SPEED_MAXJX_THRUSTINGASTERN_MOVINGRIGHT"),
            (140,"SPEED_MIDJX_THRUSTINGASTERN_MOVINGLEFT"),
            (141,"SPEED_MIDJX_THRUSTINGASTERN_MOVINGRIGHT"),
            (142,"SPEED_MINJY_DOCKINGMODE"),
            (143,"SPEED_MAXJY_DOCKINGMODE"),
            (144,"SPEED_MINJY_TRANSIT"),
            (145,"SPEED_MAXJY_TRANSIT"),
            (146,"NUM_READS_AUTOCAL"),
            (147,"PBKT_NEUTRAL_THRUST"),
            (148,"SBKT_NEUTRAL_THRUST"),
            (149,"PNOZ_CENTER_POS"),
            (150,"SNOZ_CENTER_POS"),
            (151,"TRANSVERSE_NOZCLAMP"),
            (152,"BKT_ZTHRUST"),
            (153,"NOZZLE_MIN_UNIT1"),
            (154,"NOZZLE_NEUTRAL_UNIT1"),
            (155,"NOZZLE_MAX_UNIT1"),
            (156,"NOZZLE_MIN_UNIT2"),
            (157,"NOZZLE_NEUTRAL_UNIT2"),
            (158,"NOZZLE_MAX_UNIT2"),
            (159,"BUCKET_MIN_UNIT1"),
            (160,"BUCKET_NEUTRAL_UNIT1"),
            (161,"BUCKET_MAX_UNIT1"),
            (162,"BUCKET_MIN_UNIT2"),
            (163,"BUCKET_NEUTRAL_UNIT2"),
            (164,"BUCKET_MAX_UNIT2"),
            (165,"DEFAULT_MIN_NOZZLEFDBK"),
            (166,"DEFAULT_MAX_NOZZLEFDBK"),
            (167,"DEFAULT_MIN_BUCKETFDBK"),
            (168,"DEFAULT_MAX_BUCKETFDBK"),
            (169,"TABPOSMIN_1"),
            (170,"TABPOSMAX_1"),
            (171,"TABPOSMIN_2"),
            (172,"TABPOSMAX_2"),
            (174,"DEFAULT_MIN_TABPOS"),
            (175,"DEFAULT_MAX_TABPOS"),
            (176,"RP_MIN"),
            (177,"RP_MID"),
            (178,"RP_MAX"),
            (179,"TP_MIN"),
            (180,"TP_MID"),
            (181,"TP_MAX"),
            (182,"RP_MIN_STA2"),
            (183,"RP_MID_STA2"),
            (184,"RP_MAX_STA2"),
            (185,"TP_MIN_STA2"),
            (186,"TP_MID_STA2"),
            (187,"TP_MAX_STA2"),
            (190,"MAX_TAB_STEER"),
            (191,"MAX_TAB_ROLL"),
            (192,"TAB_RETRACT_OFFSET"),
            (193,"TAB_RPM_THRESHOLD"),
            (194,"TRIM_FACTOR_TAB"),
            (195,"ROLL_FACTOR_TAB"),
            (196,"PORT_RPM_IDLE"),
            (197,"PORT_RPM_FULL"),
            (198,"STBD_RPM_IDLE"),
            (199,"STBD_RPM_FULL"),
            (200,"BSTR_RPM_IDLE"),
            (201,"BSTR_RPM_FULL"),
            (202,"MIN_DA_ENGINESPEED"),
            (203,"MID_DA_ENGINESPEED"),
            (204,"MAX_DA_ENGINESPEED"),
            (205,"PWM_TAB_MIN"),
            (206,"PWM_TAB_MAX"),
            (207,"PWM_RANGE"),
            (208,"PWM_FREQ_MIN"),
            (209,"PWM_FREQ_MAX"),
            (210,"IDLEPOT1_MIN"),
            (211,"IDLEPOT1_MAX"),
            (212,"IDLEPOT2_MIN"),
            (213,"IDLEPOT2_MAX"),
            (214,"IDLEPOT3_MIN"),
            (215,"IDLEPOT3_MAX"),
            (216,"IDLESETTING_MAX"),
            (218,"BT_TYPE"),
            (219,"BT_MIN"),
            (220,"BT_MAX"),
            (221,"BT_ZERO"),
            (222,"AUTOPILOT_AD_MIN"),
            (223,"AUTOPILOT_AD_MAX"),
            (224,"DELHELM_MIDRANGE_AP"),
            (225,"DELHELMCOM_MIDRANGE_AP"),
            (226,"AUTOPILOT_ACC_DELAY"),
            (227,"GENERAL_ALARM_REVERSE"),
            (228,"AUTO_TAB_RETRACT"),
            (233,"A_TO_D_EXPANSION"),
            (234,"BKT1_SENSOR_REVERSE"),
            (235,"BKT2_SENSOR_REVERSE"),
            (236,"DEFAULT_MAX_POTS"),
            (237,"DEFAULT_MIN_JOYSTICK"),
            (238,"DEFAULT_MIN_POTS"),
            (239,"JET_EXPRESS"),
            (240,"NOZ1_SENSOR_REVERSE"),
            (241,"NOZ2_SENSOR_REVERSE"),
            (242,"NUMBER_STATIONS"),
            (243,"PORT_STBD_CU_CONFIG"),
            (244,"PORTABLE_STATION"),
            (245,"POWER_UP_CTR_MAX"),
            (246,"REMOTE_LOCAL_MISMATCH_CTR_MAX"),
            (247,"STA3_CONFIG"),
            (248,"THRMAX_1_STA3"),
            (249,"THRMID_1_STA3"),
            (250,"THRMIN_1_STA3"),
        };
    }
}

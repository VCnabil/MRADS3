using MRADS2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRADS2.Panels;
using MRADS2.Panels.CCM;
using MRADS2.Ships.CCM;
using MRADS2.Ships;
using static MRADS2.Ships.CCM.CCM;
using MRADS2.Ships.StandardShip;
using MRADS2.Ships.ViewModel;

namespace MRADS.Ships.SANFRAN
{
    public class SANFRANCISCO : MRADSShip
    {
        public SANFRANCISCO() { }
        public List<PDM> PDMs = new List<PDM>();

        public override IEnumerable<BasePanel> GetPanels(MRADSDataVM vmdata)
        {
            List<BasePanel> ret = new List<BasePanel>();

            var shipvm = (SANFRANVM)ShipViewModel;

            ret.Add(new LCDIndicationPanel(vmdata, shipvm));
       

            return (ret);
        }

        protected void InitEvents()
        {
 
        }
        protected override void InitShip()
        {
 
            InitEvents();
        }
        protected override void InitControlUnit(MRADSControlUnit CU)
        {
            PGNDecoder decoder;
            var prefixes = (string[])CU.Data;
            string prefix1 = prefixes[0], prefix2 = prefixes[1];

             

            var pdecoder = CU.AddParameterPGN(0xff75);
            foreach (var p in ParameterList)
                pdecoder.SetParameter(p.index, p.name);

          
        }

        void InitFaults(PGNDecoder decoder, string prefix1, string prefix2)
        {
       
        }

        protected override void InitClutchPanel(MRADSClutchPanel CP)
        {
            PGNDecoder decoder;
            var prefixes = (string[])CP.Data;
            string prefix1 = prefixes[0], prefix2 = prefixes[1];
 
        }


        protected virtual void AdditionalInit()
        {
           
        }
        MRADSDataProvider InitGPS(string name, int channel)
        {
            MRADSDataProvider gps = Config.CreateProvider(name, 0x7f, channel);
            PGNDecoder decoder;
 

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
 

            return (vmu);
        }

        uint ToUInt24(byte[] b, int index)
        {
            return (b[index] | ((uint)b[index + 1] << 8) | ((uint)b[index + 2] << 16));
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

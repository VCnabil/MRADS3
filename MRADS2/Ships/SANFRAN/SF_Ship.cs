﻿using MRADS2;
using MRADS2.Panels.CCM;
using MRADS2.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRADS2.Panels.SF;
using MRADS2.Ships.StandardShip;
using MRADS2.Ships.ViewModel;
using MRADS2.Ships.CCM;
using MRADS2.Ships;
using MRADS2.Panels.StandardShip;

namespace MRADS.Ships.SANFRAN
{
    public class SF_Ship  : MRADSShip
    {

       

        #region BaseImplementatuon
        public override IEnumerable<BasePanel> GetPanels(MRADSDataVM vmdata)
        {
            List<BasePanel> ret = new List<BasePanel>();

            var shipvm = (SFEXAM_VM)ShipViewModel;

            ret.Add(new SF_Pannel_A(vmdata, shipvm));
           // ret.Add(new SANFRAN_EXAMPAN(vmdata, shipvm));
            ret.Add(new GPSPanel(vmdata, shipvm.GPS));


            return (ret);
        }
        protected override DefaultBindVM GetViewModel(MRADSDataVM vmship, MRADSShipConfig config)
        {
            return (new SFEXAM_VM(vmship, config));
        }


        protected override void InitControlUnit(MRADSControlUnit CU)
        {
            PGNDecoder decoder;

            decoder = CU.AddPGN(0xff8c);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MajorSWVersion", 0, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MinorSWVersion", 1, 2));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("SWRevision", 3, 4));

            decoder = CU.AddPGN(0xff30);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("cluAHEcmd_S", 3, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("cluAHEcmd_P", 3, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("cluREVcmd_S", 3, 4));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("cluREVcmd_P", 3, 5));
            //cluREV_S_boolVar  cluREV_P_boolVar



            decoder = CU.AddPGN(0xff8d);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("ActiveCuID", 5, 5));

            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("bP_IOSKIM_Fault", 6, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("bS_IOSKIM_Fault", 6, 1));

            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("CrashStop", 7, 7));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("bPCrashStopFlag", 7, 0));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("bSCrashStopFlag", 7, 1));

        }

        protected override void InitClutchPanel(MRADSClutchPanel CP)
        {
            PGNDecoder Clutchdecoder;

            Clutchdecoder = CP.AddPGN(0xff89);
            Clutchdecoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("CluSW_MAJ", 0, 0));
            Clutchdecoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("CluSW_min", 1, 1));
            Clutchdecoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("CluSW_REV", d => (d[2] << 8) | d[3]));

            Clutchdecoder = CP.AddPGN(0xff31);
            Clutchdecoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("cluAHE_S", 0, 0));
            Clutchdecoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("cluAHE_P", 0, 1));
            Clutchdecoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("cluTRO_S", 0, 2));
            Clutchdecoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("cluTRO_P", 0, 3));
            Clutchdecoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("cluREV_S", 0, 4));
            Clutchdecoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("cluREV_P", 0, 5));
            Clutchdecoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("CLU_P_STATE", 5, 5));
            Clutchdecoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("CLU_S_STATE", 6, 6));




        }
        protected void InitEngine(string name, byte source, int channel)
        {
            MRADSDataProvider engine = Config.CreateProvider(name, source, channel);
            PGNDecoder decoder;

            decoder = engine.AddPGN(0xff8f);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Engine1RPM", d => (d[5] << 8) | d[4]));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Engine2RPM", d => (d[7] << 8) | d[6]));           
        }

        //at address 80 
        protected void Init_Skim80(string name, byte source, int channel) {
            MRADSDataProvider skimer_80 = Config.CreateProvider(name, source, channel);
            PGNDecoder skimerdecoder_80;
            skimerdecoder_80 = skimer_80.AddPGN(0xffa2);
            skimerdecoder_80.AddVariableDefinition(MRADSVariableDefinition.CreateInt("skim80rpm", d => (d[1] << 8) | d[0]));

            skimerdecoder_80 = skimer_80.AddPGN(0xffa7);  
            skimerdecoder_80.AddVariableDefinition(MRADSVariableDefinition.CreateBool("skim80bool", d => d[0] == 0xfc)); //if manual we get true anytging else wil lbe false
          
        }
        //at address 81 
        protected void Init_Skim81(string name, byte source, int channel)
        {
            MRADSDataProvider skimer_81= Config.CreateProvider(name, source, channel);
            PGNDecoder skimerdecoder_81;
            skimerdecoder_81 = skimer_81.AddPGN(0xffa0);
            skimerdecoder_81.AddVariableDefinition(MRADSVariableDefinition.CreateInt("skim81rpm", d => (d[1] << 8) | d[0]));

            skimerdecoder_81 = skimer_81.AddPGN(0xffa6);
            skimerdecoder_81.AddVariableDefinition(MRADSVariableDefinition.CreateBool("skim81bool", d => d[0] == 0xfc)); //if manual we get true anytging else wil lbe false
        }
        
        protected void Init_GPS(string name, byte source, int channel) { 
            MRADSDataProvider gps = Config.CreateProvider(name, source, channel);
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

        }
        DateTime ParseGPSTime(byte[] data)
        {
            uint days, ticks;

            days = BitConverter.ToUInt16(data, 2);
            ticks = BitConverter.ToUInt32(data, 4);

            return (DateTime.UnixEpoch.AddDays(days).AddMilliseconds(ticks / 10.0));
        }

        protected override bool CheckShipID(MRADSShip ship)
        {
            var v = ship.State.GetValue(ship.ControlUnits[0], "MajorSWVersion");
            if (!v.Set)
                return (false);

            return (v.GetValue<int>() == 67);
        }
    
        #endregion
        public SF_Ship( )  
        {
            //5057877.54120,2,18ff8c29,4,8,43,f2,03,7c,11,00,00,00,6137442,2024-05-25 05:54:48.1524
            //            ch2
            //                pgn ff8c
            //                          byte0=Major version 0x43 or 67 in decimal
            ControlUnits.Add(new MRADSControlUnit("MAIN Control Unit", 0x29,  null, 2));

            //5057885.68423,2,cff8934,4,8,02,00,14,99,00,00,00,00,6141400,2024-05-25 05:54:56.2954
            //             ch2
            //                pgn ff89
            //                          byte5=Major version 0x02 or 2 in decimal
            ClutchPanels.Add(new MRADSClutchPanel("Clutch Panel", 0x34, null, 2));
           
        }

        protected override void InitShip()
        { 
            InitControlUnit(ControlUnits[0]);
            InitClutchPanel(ClutchPanels[0]);
            InitEngine("MyEngine", 0x29, 2);
            Init_Skim80("MySkim80", 0x80, 2);
            Init_Skim81("MySkim81", 0x81, 2);
            Init_GPS("GPS", 0x7f, 1);

            InitEvents();

        }
        protected void InitEvents()
        {
            AddEventHandler(new StandardMessageEvent(this));
            AddEventHandler(new StandardOnlineOfflineEvent(this));
            //AddEventHandler(new StandardShipFaultEvent(this));
            //AddEventHandler(new StandardShipVariableChangeEvent(this));
            //AddEventHandler(new StandardShipCalibrationEvent(this));

        }

        
    }
}

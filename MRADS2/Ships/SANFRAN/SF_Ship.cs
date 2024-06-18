using MRADS2;
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

namespace MRADS.Ships.SANFRAN
{
    public class SF_Ship  : MRADSShip
    {

       

        #region BaseImplementatuon
        public override IEnumerable<BasePanel> GetPanels(MRADSDataVM vmdata)
        {
            List<BasePanel> ret = new List<BasePanel>();

            var shipvm = (SFEXAM_VM)ShipViewModel;

            ret.Add(new SANFRAN_EXAMPAN(vmdata, shipvm));


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

        }
        protected override void InitClutchPanel(MRADSClutchPanel CP)
        {
            PGNDecoder decoder;

            decoder = CP.AddPGN(0xff89);
     

            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MajorSWVersion", 5, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("MinorSWVersion", 6, 6));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("SWRevision", 7, 7));

        }
        protected void InitEngine(string name, byte source, int channel)
        {
            MRADSDataProvider engine = Config.CreateProvider(name, source, channel);
            PGNDecoder decoder;

            decoder = engine.AddPGN(0xff8f);
            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Engine1RPM", d => (d[5] << 5) | (d[4] >> 3)));
            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Engine2RPM", d => (d[7] << 5) | (d[6] >> 3)));

            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Engine1RPM", d => (d[5] << 8) | d[4]));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Engine2RPM", d => (d[7] << 8) | d[6]));


            //decoder = engine.AddPGN(0xf005);
            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("CurrentGearAHD", d => d[3] == 126 || d[3] == 127));
            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("CurrentGearREV", d => d[3] == 124));
            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("CurrentGearNEU", d => d[3] != 124 && d[3] != 126 && d[3] != 127));

            //decoder = engine.AddPGN(0xfef8);
            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("ClutchPressure", d => d[0] * 16 * 0.145038));
            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("TransmissionOilTemperature", d => (((d[5] << 8) | (d[4])) * 0.03125) - 273));

            //decoder = engine.AddPGN(0xfeef);
            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("EngineOilPressure", d => d[3] * 4 * 0.145038));

            //decoder = engine.AddPGN(0xfee5);
            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("TotalEngineHours", d => (int)(BitConverter.ToUInt32(d, 0) * 0.05)));

            //decoder = engine.AddPGN(0xfef7);
            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateFloat("BatteryVoltage", d => BitConverter.ToUInt16(d, 6) * 0.05));

            //decoder = engine.AddPGN(0xf003);
            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("EngineLoad", 2, 2));

            //decoder = engine.AddPGN(0xfedf);
            //decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("DesiredRPM", d => (d[2] << 5) | (d[1] >> 3)));
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

        }
        protected void InitEvents()
        {
            //AddEventHandler(new StandardMessageEvent(this));
            //AddEventHandler(new StandardOnlineOfflineEvent(this));
            //AddEventHandler(new StandardShipFaultEvent(this));
            //AddEventHandler(new StandardShipVariableChangeEvent(this));
            //AddEventHandler(new StandardShipCalibrationEvent(this));

        }

        
    }
}

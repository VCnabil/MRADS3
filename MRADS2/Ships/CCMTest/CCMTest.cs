using MRADS2.Panels;
using MRADS2.Ships.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRADS2.Panels.CCM;

namespace MRADS2.Ships.CCMTest
{
    class CCMNew : CCM.CCM
    {
        public CCMNew() : base()
        {
        }

        protected override DefaultBindVM GetViewModel(MRADSDataVM vmship, MRADSShipConfig config) => new CCMTestVM(vmship, config);

        public override IEnumerable<BasePanel> GetPanels(MRADSDataVM vmdata)
        {
            List<BasePanel> ret = new List<BasePanel>();

            var shipvm = (CCMTestVM)ShipViewModel;

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
            ret.Add(new Panels.StandardShip.VMUPanel(vmdata, shipvm.VMU));
            ret.Add(new Panels.StandardShip.GPSPanel(vmdata, shipvm.GPS));
            ret.Add(new PDMInputsPanel(vmdata, shipvm));
            ret.Add(new PDMOutputsPanel(vmdata, shipvm));
            ret.Add(new VariableList(vmdata));

            return (ret);
        }

        protected override void InitShip()
        {
            InitGPS("GPS", 1);
            InitVMU("VMU", 1);

            InitEngine("Port Engine", 0x00, 2);
            InitEngine("Starboard Engine", 0x01, 2);

            InitEngineController("Port Engine Controller", 0x00, 2);
            InitEngineController("Starboard Engine Controller", 0x01, 2);

            InitSKIM("Panel Interface Module", 0x80, 2);

            InitMurphyLCD("Murphy LCD", 0x50, 1);

            InitPDMs(4);

            InitEvents();
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

        protected override bool CheckShipID(MRADSShip ship)
        {
            var v = ship.State.GetValue(ship.ControlUnits[0], "MajorSWVersion");
            if (!v.Set)
                return false;

            return v.GetValue<int>() == 53;
        }
    }
}

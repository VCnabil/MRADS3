using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MRADS2;
using MRADS2.Ships.GenericViewModels;
using MRADS2.Ships.StandardShip;
using MRADS2.Ships.ViewModel;

namespace MRADS.Ships.SANFRAN
{
    public class SANFRANVM : DataVMProvider, DefaultBindVM
    {
        string name_cu;
        public GPSVM GPS { get; }
        public BindVariable<string> RollAngle { get; private set; }
        public BindVariable<string> PitchAngle { get; private set; }
        public BindVariable<string> AngleLatency { get; private set; }

        public BindVariable<string> PitchRate { get; private set; }
        public BindVariable<string> RollRate { get; private set; }
        public BindVariable<string> YawRate { get; private set; }
        public BindVariable<string> RateLatency { get; private set; }

        public BindVariable<string> XAxisAcceleration { get; private set; }
        public BindVariable<string> YAxisAcceleration { get; private set; }
        public BindVariable<string> ZAxisAcceleration { get; private set; }

        public MRADSControlUnit ControlUnit;
        public MRADSClutchPanel ClutchPanel;

        public BindVariable ThrottleAnalogInput { get; private set; }

        public SANFRANVM(MRADSDataVM datavm, MRADSShipConfig config, bool oldvmu = true) : base(datavm)
        {

            GPS = new GPSVM();
            GPS.DefaultBind(datavm);
            DefaultBind(datavm);

        }
        public void DefaultBind(MRADSDataVM datavm)
        {
              
            RollAngle = datavm.GetVariable("VMU", "RollAngle").Bind(v => ((double)v.RawValue).ToString("0.000"));
            PitchAngle = datavm.GetVariable("VMU", "PitchAngle").Bind(v => ((double)v.RawValue).ToString("0.000"));
            AngleLatency = datavm.GetVariable("VMU", "AngleLatency").Bind(v => ((double)v.RawValue).ToString("0.0"));

            PitchRate = datavm.GetVariable("VMU", "PitchRate").Bind(v => ((double)v.RawValue).ToString("0.000"));
            RollRate = datavm.GetVariable("VMU", "RollRate").Bind(v => ((double)v.RawValue).ToString("0.000"));
            YawRate = datavm.GetVariable("VMU", "YawRate").Bind(v => ((double)v.RawValue).ToString("0.000"));
            RateLatency = datavm.GetVariable("VMU", "RateLatency").Bind(v => ((double)v.RawValue).ToString("0.0"));

            XAxisAcceleration = datavm.GetVariable("VMU", "XAxisAcceleration").Bind(v => ((double)v.RawValue).ToString("0.00"));
            YAxisAcceleration = datavm.GetVariable("VMU", "YAxisAcceleration").Bind(v => ((double)v.RawValue).ToString("0.00"));
            ZAxisAcceleration = datavm.GetVariable("VMU", "ZAxisAcceleration").Bind(v => ((double)v.RawValue).ToString("0.00"));
            ThrottleAnalogInput = datavm.GetVariable(name_cu, $"ThrottleAnalogInput").Bind();


            var prefixes = (string[])ControlUnit.Data;
            string prefix1, prefix2;

            prefix1 = prefixes[0];
            prefix2 = prefixes[1];
        }
        static public string GetStationInControlName(int index)
        {
            switch (index)
            {
                case 1:
                    return ("CU actual CU 29");
                case 2:
                    return ("AM unit 80");
                case 3:
                    return ("AM unit 81");
                default:
                    return ("Unknown");
            }
        }
    }
}

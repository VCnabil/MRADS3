using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRADS2.Ships.ViewModel;

namespace MRADS2.Ships.GenericViewModels
{
    public class VMUVM : DefaultBindVM
    {
        //working
        public BindVariable<string> RollAngle { get; private set; }
        public BindVariable<string> PitchAngle { get; private set; }
        public BindVariable<string> YawAngle { get; private set; }
        public BindVariable<string> AngleLatency { get; private set; }

        public BindVariable<string> PitchRate { get; private set; }
        public BindVariable<string> RollRate { get; private set; }
        public BindVariable<string> YawRate { get; private set; }
        public BindVariable<string> RateLatency { get; private set; }

        public BindVariable<string> XAxisAcceleration { get; private set; }
        public BindVariable<string> YAxisAcceleration { get; private set; }
        public BindVariable<string> ZAxisAcceleration { get; private set; }

        public BindVariable<string> LCDBoatSpeed { get; private set; }

        public void DefaultBind(MRADSDataVM datavm)
        {
            RollAngle = datavm.GetVariable("VMU", "RollAngle").Bind(v => ((double)v.RawValue).ToString("0.000"));
            PitchAngle = datavm.GetVariable("VMU", "PitchAngle").Bind(v => ((double)v.RawValue).ToString("0.000"));
            YawAngle = datavm.GetVariable("VMU", "YawAngle").Bind(v => ((double)v.RawValue).ToString("0.000"));
            AngleLatency = datavm.GetVariable("VMU", "AngleLatency").Bind(v => ((double)v.RawValue).ToString("0.0"));

            PitchRate = datavm.GetVariable("VMU", "PitchRate").Bind(v => ((double)v.RawValue).ToString("0.000"));
            RollRate = datavm.GetVariable("VMU", "RollRate").Bind(v => ((double)v.RawValue).ToString("0.000"));
            YawRate = datavm.GetVariable("VMU", "YawRate").Bind(v => ((double)v.RawValue).ToString("0.000"));
            RateLatency = datavm.GetVariable("VMU", "RateLatency").Bind(v => ((double)v.RawValue).ToString("0.0"));

            XAxisAcceleration = datavm.GetVariable("VMU", "XAxisAcceleration").Bind(v => ((double)v.RawValue).ToString("0.00"));
            YAxisAcceleration = datavm.GetVariable("VMU", "YAxisAcceleration").Bind(v => ((double)v.RawValue).ToString("0.00"));
            ZAxisAcceleration = datavm.GetVariable("VMU", "ZAxisAcceleration").Bind(v => ((double)v.RawValue).ToString("0.00"));

            LCDBoatSpeed= datavm.GetVariable("Murphy LCD", "BoatSpeed").Bind(v => ((double)v.RawValue).ToString("0.0"));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRADS2.Ships.ViewModel;

namespace MRADS2.Ships.GenericViewModels
{
    public class GPSVM : DefaultBindVM
    {
        public BindVariable<string> Heading { get; private set; }
        public BindVariable<string> RateOfTurn { get; private set; }
        public BindVariable<string> SpeedOverGround { get; private set; }
        public BindVariable<string> CourseOverGround { get; private set; }

        public BindVariables<string> GPSPosition { get; private set; }
        public BindVariables<string> GPSDateTime { get; private set; }

        public void DefaultBind(MRADSDataVM datavm)
        {
            Heading = datavm.GetVariable("GPS", "Heading").Bind(v => ((double)v.RawValue).ToString("0.000"));
            RateOfTurn = datavm.GetVariable("GPS", "RateOfTurn").Bind(v => ((double)v.RawValue).ToString("0.0000"));
            SpeedOverGround = datavm.GetVariable("GPS", "SpeedOverGround").Bind(v => ((double)v.RawValue).ToString("0.00"));
            CourseOverGround = datavm.GetVariable("GPS", "CourseOverGround").Bind(v => ((double)v.RawValue).ToString("0.0"));

            GPSPosition = datavm.MultiBind(v => $"{(double)v["Latitude"].RawValue:0.0000}, {(double)v["Longitude"].RawValue:0.0000}", "Longitude", "Latitude");
            GPSDateTime = datavm.MultiBind(v =>
            {
                DateTime dt = new DateTime((int)v["Year"].RawValue, (int)v["Month"].RawValue, (int)v["Day"].RawValue, (int)v["Hour"].RawValue, (int)v["Minute"].RawValue, (int)v["Second"].RawValue);

                return (dt.ToString("yyyy-MM-dd HH:mm:ss"));
            }, "Year", "Month", "Day", "Hour", "Minute", "Second");
        }
    }
}

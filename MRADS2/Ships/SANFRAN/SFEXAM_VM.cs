using MRADS2.Ships.ViewModel;
using MRADS2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRADS2.Ships.StandardShip;
using System.Threading;
using MRADS2.Ships.CCM;


namespace MRADS.Ships.SANFRAN
{
    public class SFEXAM_VM : DataVMProvider, DefaultBindVM
    {
        public MAIN_CUVM MainCU { get; }
        public EngineVM MainEngine { get; }

        public SFEXAM_VM(MRADSDataVM vmdata ,MRADSShipConfig config, bool oldvmu = true) : base(vmdata)
        {
            MainCU = new MAIN_CUVM(vmdata.Ship.ControlUnits[0]);
            MainEngine = new EngineVM(config.GetProvider("MyEngine"));
          
        }
    }

    public class MAIN_CUVM : DefaultBindVM
    {
        public BindVariables<string> SoftwareVersion { get; private set; }
        MRADSControlUnit ControlUnit;
        public MAIN_CUVM(MRADSControlUnit controlunit)
        {
            ControlUnit = controlunit;
        }
        public void DefaultBind(MRADSDataVM datavm)
        {
            SoftwareVersion = datavm.MultiBind(ControlUnit.Name, v => $"{v["MajorSWVersion"].Value}.{v["MinorSWVersion"].Value} Rev{v["SWRevision"].Value}", "MajorSWVersion", "MinorSWVersion", "SWRevision");
        }
    }

    public class EngineVM : DefaultBindVM
    {
        public BindVariable Eng_1_RPM { get; private set; }
        public BindVariable Eng_2_RPM { get; private set; }

       

        MRADSDataProvider EngineDP;

        public EngineVM(MRADSDataProvider engine)
        {
            EngineDP = engine;
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            Eng_1_RPM = datavm.GetVariable(EngineDP.Name, "Engine1RPM").Bind();
            Eng_2_RPM = datavm.GetVariable(EngineDP.Name, "Engine2RPM").Bind();


        }
    }
}

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

        public Skim80VM MainSkim80 { get; }
        public Skim81VM MainSkim81 { get; }

        public SFEXAM_VM(MRADSDataVM vmdata ,MRADSShipConfig config, bool oldvmu = true) : base(vmdata)
        {
            MainCU = new MAIN_CUVM(vmdata.Ship.ControlUnits[0]);
            MainEngine = new EngineVM(config.GetProvider("MyEngine"));
            MainSkim80 = new Skim80VM(config.GetProvider("MySkim80"));
            MainSkim81 = new Skim81VM(config.GetProvider("MySkim81"));

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

    //make a skim80VM


    public class Skim80VM : DefaultBindVM
    {
        public BindVariable Skim80rpmVar { get; private set; }
        public BindVariable Skim80boolVar { get; private set; }

        MRADSDataProvider Skim80DP;
        public Skim80VM(MRADSDataProvider argskim)
        {
            Skim80DP = argskim;    
        }
        public void DefaultBind(MRADSDataVM datavm)
        {
            Skim80rpmVar = datavm.GetVariable(Skim80DP.Name, "skim80rpm").Bind();
            Skim80boolVar = datavm.GetVariable(Skim80DP.Name, "skim80bool").Bind();
        }
    }

    public class Skim81VM : DefaultBindVM
    {
        public BindVariable Skim81rpmVar { get; private set; }
        public BindVariable Skim81boolVar { get; private set; }

        MRADSDataProvider Skim81DP;
        public Skim81VM(MRADSDataProvider argskim)
        {
            Skim81DP = argskim;
        }
        public void DefaultBind(MRADSDataVM datavm)
        {
            Skim81rpmVar = datavm.GetVariable(Skim81DP.Name, "skim81rpm").Bind();
            Skim81boolVar = datavm.GetVariable(Skim81DP.Name, "skim81bool").Bind();
        }
    }

   
}

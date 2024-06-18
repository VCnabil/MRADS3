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
using MRADS2.Panels.StandardShip;


namespace MRADS.Ships.SANFRAN
{
    public class SFEXAM_VM : DataVMProvider, DefaultBindVM
    {

        public MAIN_CUVM MainCU { get; }
        public EngineVM MainEngine { get; }

        public Skim80VM MainSkim80 { get; }
        public Skim81VM MainSkim81 { get; }

        public MAIN_ClutchVM MAIN_Clutch { get; }

        public SFEXAM_VM(MRADSDataVM vmdata ,MRADSShipConfig config, bool oldvmu = true) : base(vmdata)
        {
            MainCU = new MAIN_CUVM(vmdata.Ship.ControlUnits[0]);
            MainEngine = new EngineVM(config.GetProvider("MyEngine"));
            MainSkim80 = new Skim80VM(config.GetProvider("MySkim80"));
            MainSkim81 = new Skim81VM(config.GetProvider("MySkim81"));
            MAIN_Clutch = new MAIN_ClutchVM(vmdata.Ship.ClutchPanels[0]);
        }
    }
     

    public class MAIN_CUVM : DefaultBindVM
    {
        public BindVariables<string> SoftwareVersion { get; private set; }

        public BindVariable cluAHEcmd_S_boolVar { get; private set; }
        public BindVariable cluAHEcmd_P_boolVar { get; private set; }
        public BindVariable cluTROcmd_S_boolVar { get; private set; }
        public BindVariable cluTROcmd_P_boolVar { get; private set; }
        public BindVariable cluREVcmd_S_boolVar { get; private set; }
        public BindVariable cluREVcmd_P_boolVar { get; private set; }



        public BindVariable ActiveCuIDVar { get; private set; }
        public BindVariable CrashStopVar { get; private set; }

        MRADSControlUnit ControlUnit;
        public MAIN_CUVM(MRADSControlUnit controlunit)
        {
            ControlUnit = controlunit;
        }
        public void DefaultBind(MRADSDataVM datavm)
        {
            SoftwareVersion = datavm.MultiBind(ControlUnit.Name, v => $"{v["MajorSWVersion"].Value}.{v["MinorSWVersion"].Value} Rev{v["SWRevision"].Value}", "MajorSWVersion", "MinorSWVersion", "SWRevision");
        

            cluAHEcmd_S_boolVar = datavm.GetVariable(ControlUnit.Name, "cluAHEcmd_S").Bind();
            cluAHEcmd_P_boolVar = datavm.GetVariable(ControlUnit.Name, "cluAHEcmd_P").Bind();
            cluTROcmd_S_boolVar = datavm.GetVariable(ControlUnit.Name, "cluTROcmd_S").Bind();
            cluTROcmd_P_boolVar = datavm.GetVariable(ControlUnit.Name, "cluTROcmd_P").Bind();
            cluREVcmd_S_boolVar = datavm.GetVariable(ControlUnit.Name, "cluREVcmd_S").Bind();
            cluREVcmd_P_boolVar = datavm.GetVariable(ControlUnit.Name, "cluREVcmd_P").Bind();

            ActiveCuIDVar = datavm.GetVariable(ControlUnit.Name, "ActiveCuID").Bind();
            CrashStopVar = datavm.GetVariable(ControlUnit.Name, "CrashStop").Bind();
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

   public class MAIN_ClutchVM : DefaultBindVM
    {
        public BindVariables<string> cluSWversion { get; private set; }



        public BindVariable cluAHE_S_boolVar { get; private set; }
        public BindVariable cluAHE_P_boolVar { get; private set; }
        public BindVariable cluTRO_S_boolVar { get; private set; }
        public BindVariable cluTRO_P_boolVar { get; private set; }
        public BindVariable cluREV_S_boolVar { get; private set; }
        public BindVariable cluREV_P_boolVar { get; private set; }


        public BindVariable clu_s_stateVar { get; private set; }
        public BindVariable clu_p_stateVar { get; private set; }

        MRADSClutchPanel ClutchPanel;
        public MAIN_ClutchVM(MRADSClutchPanel clutchpanel)
        {
            ClutchPanel = clutchpanel;
        }
        public void DefaultBind(MRADSDataVM datavm)
        {
            cluSWversion = datavm.MultiBind(ClutchPanel.Name, v => $"{v["CluSW_MAJ"].Value}.{v["CluSW_min"].Value} Rev{v["CluSW_REV"].Value}", "CluSW_MAJ", "CluSW_min", "CluSW_REV");

            cluAHE_S_boolVar = datavm.GetVariable(ClutchPanel.Name, "cluAHE_S").Bind();
            cluAHE_P_boolVar = datavm.GetVariable(ClutchPanel.Name, "cluAHE_P").Bind();
            cluTRO_S_boolVar = datavm.GetVariable(ClutchPanel.Name, "cluTRO_S").Bind();
            cluTRO_P_boolVar = datavm.GetVariable(ClutchPanel.Name, "cluTRO_P").Bind();
            cluREV_S_boolVar = datavm.GetVariable(ClutchPanel.Name, "cluREV_S").Bind();
            cluREV_P_boolVar = datavm.GetVariable(ClutchPanel.Name, "cluREV_P").Bind();
            clu_s_stateVar = datavm.GetVariable(ClutchPanel.Name, "CLU_S_STATE").Bind();
            clu_p_stateVar = datavm.GetVariable(ClutchPanel.Name, "CLU_P_STATE").Bind();


        }
    }
}

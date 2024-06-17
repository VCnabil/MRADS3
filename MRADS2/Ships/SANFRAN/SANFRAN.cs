using MRADS2.Panels.CCM;
using MRADS2.Panels;
using MRADS2.Ships.CCM;
using MRADS2.Ships.StandardShip;
using MRADS2.Ships.ViewModel;
using MRADS2.Ships;
using MRADS2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static MRADS2.Ships.CCM.CCM;
using System.Windows;

namespace MRADS.Ships.SANFRAN
{
    public class SANFRAN : MRADSShip
    {
        


        public override IEnumerable<BasePanel> GetPanels(MRADSDataVM vmdata)
        {
            List<BasePanel> ret = new List<BasePanel>();

            var shipvm = (CCMVM)ShipViewModel;


           ret.Add(new VersionsPanel(vmdata, shipvm));


            return (ret);
        }
        protected void InitEvents()
        {
            AddEventHandler(new StandardMessageEvent(this));
            AddEventHandler(new StandardOnlineOfflineEvent(this));
        }

        public SANFRAN()
        {
            ControlUnits.Add(new MRADSControlUnit(" A or B Control Unit", 0x29, null, 2));
            ClutchPanels.Add(new MRADSClutchPanel("Clutch Panel", 0x34, null, 2));
        }
        protected void InitPDMs(int channel)
        {
            PDM pdm;

            pdm = new PDM("PDM 24V 1A", 0x01, channel);

            // Inputs
            pdm.AddInput(0, "Engine Compt Flood Level Switch");
            pdm.AddInput(1, "Port Engine Running");
            pdm.AddInput(2, "Port Frame 9 Bilge Float Switch");
            pdm.AddInput(3, "Port Engine Compt Bilge Float Switch");
            pdm.AddInput(8, "Sea Water Pressure Element, Port");
            pdm.AddInput(9, "Differential Pressure (Vaccum), Port");
            pdm.AddInput(10, "Port Frame 9 Bilge Pump Run Signal");
            pdm.AddInput(11, "Port Engine Compt Bilge Pump Run Signal");

            // Outputs
            pdm.AddOutput(0, "Bilge Pump, Frame 9 Port");
            pdm.AddOutput(1, "Bilge Pump, Eng. Compt. Port");
            pdm.AddOutput(2, "Bilge Blower Port");
            pdm.AddOutput(3, "Elex Cooling AC Pump");
            pdm.AddOutput(6, "MTU CCU Port Primary");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 1B", 0x02, channel);

            // Inputs
            pdm.AddInput(10, "Port Seachest Priming Pump Run Signal");
            pdm.AddInput(7, "Port Engine Fuel Prime Pump Enable");
            pdm.AddInput(5, "Hydraulic Oil Filter Clogged, Port Drives System");
            pdm.AddInput(8, "Hydraulic System Pressure, Tabs");
            pdm.AddInput(4, "Hydraulic Oil Filter Clogged, Port Tabs System");
            pdm.AddInput(9, "Port Frame 12 Bilge Pump Run Signal");
            pdm.AddInput(1, "Tabs Hydraulic System Low Pressure Switch");
            pdm.AddInput(3, "Tabs Hydraulic Oil Reservoir Low Level");
            pdm.AddInput(6, "Port Frame 12 Bilge Float Switch");
            pdm.AddInput(2, "Tabs Hydraulic Oil High Temperature Switch");

            // Outputs
            pdm.AddOutput(6, "MTU CCU Stbd Secondary (Bkp)");
            pdm.AddOutput(0, "Ventilation Fan, Port");
            pdm.AddOutput(1, "Port Engine, Fuel Pump");
            pdm.AddOutput(7, "Port Seachest Priming Pump");
            pdm.AddOutput(4, "Bilge Pump, Frame 12 Port");
            pdm.AddOutput(3, "Battery Parallel Solenoid Relay, Port");
            pdm.AddOutput(2, "Crew Compt Lights, Port, Red/Green");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 4A", 0x09, channel);

            // Inputs
            pdm.AddInput(9, "Differential Pressure (Vacuum), Stbd");
            pdm.AddInput(6, "Drives Hydraulic Oil Reservoir Low Level Switch");
            pdm.AddInput(4, "Hydraulic Oil Filter Clogged, Stbd Tabs System");
            pdm.AddInput(7, "Hydraulic Oil Filter Clogged, Stbd Drives System");
            pdm.AddInput(10, "Stbd Frame 9 Bilge Pump Run Signal");
            pdm.AddInput(3, "Drives Hydraulic System Low Pressure Switch");
            pdm.AddInput(8, "Seawater Pressure Element, Stbd");
            pdm.AddInput(0, "Bilge High Level, Frame 14");
            pdm.AddInput(11, "Stbd Engine Compt Bilge Pump Run Signal");
            pdm.AddInput(2, "Engine Compt. Fire Suppression System Activated");
            pdm.AddInput(5, "Drives Hydraulic Oil High Temperature Switch");
            pdm.AddInput(1, "Stbd Engine Running");

            // Outputs
            pdm.AddOutput(0, "Bilge Pump, Frame 9 Stbd");
            pdm.AddOutput(1, "Bilge Pump, Engine Compt Stbd");
            pdm.AddOutput(6, "MTU CCU Stbd Secondary");
            pdm.AddOutput(3, "Engine Compt. Lights, Red/Green");
            pdm.AddOutput(2, "Bilge Blower, Stbd");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 4B", 0x0a, channel);

            // Inputs
            pdm.AddInput(10, "Stbd Seachest Priming Pump Run");
            pdm.AddInput(7, "Stbd Engine Fuel Prime Pump Enable");
            pdm.AddInput(4, "Stbd Engine Compt Bilge Float Switch");
            pdm.AddInput(8, "Hydraulic System Pressure, Drives");
            pdm.AddInput(3, "Stbd Frame 9 Bilge Float Switch");
            pdm.AddInput(9, "Stbd Frame 12 Bilge Pump Run Signal");
            pdm.AddInput(0, "Genset Running");
            pdm.AddInput(2, "Engine Comp R/G Lights Control");
            pdm.AddInput(5, "Stbd Frame 12 Bilge Float Switch");
            pdm.AddInput(1, "Genset Alarm");

            // Outputs
            pdm.AddOutput(6, "MTU CCU Port Secondary (Bkp)");
            pdm.AddOutput(0, "Ventilation Fan, Stbd");
            pdm.AddOutput(1, "Stbd Engine, Fuel Pump");
            pdm.AddOutput(7, "Stbd Seachest Priming Pump");
            pdm.AddOutput(4, "Bilge Pump, Frame 12 Stbd");
            pdm.AddOutput(3, "Battery Parallel Relay, Stbd");
            pdm.AddOutput(2, "Crew Compt. Lights Red/Green");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 2A", 0x03, channel);

            // Inputs
            pdm.AddInput(9, "Port Storage Compt Bilge Float Switch (Convert)");
            pdm.AddInput(6, "Alarm Silence Control");
            pdm.AddInput(4, "Engine Comp Vent Fans Control");
            pdm.AddInput(7, "Common Breaker Reset");
            pdm.AddInput(3, "Ballast Tank Dewater Pump Control");
            pdm.AddInput(8, "Port Storage Compt Bilge Pump Run Signal");
            pdm.AddInput(0, "Ballast Valve Control");
            pdm.AddInput(2, "Ballast Pump Control (Deballast)");
            pdm.AddInput(5, "Bilge Blowers Control");
            pdm.AddInput(1, "Ballast Pump Control (Ballast)");

            // Outputs
            pdm.AddOutput(2, "Bilge Pump, Storage Comp Port");
            pdm.AddOutput(3, "Storage Comp Lights Red/Green");
            pdm.AddOutput(7, "Elex Cooling AC Compressor #2");
            pdm.AddOutput(6, "Elex Cooling AC Compressor #1");
            pdm.AddOutput(4, "Bilge Power, Storage Comp");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 3A", 0x06, channel);

            // Inputs
            pdm.AddInput(6, "Ballast Tank Level 1");
            pdm.AddInput(4, "Ballast Tank Level 3");
            pdm.AddInput(8, "Stbd Storage Compt Bilge Pump Run Signal");
            pdm.AddInput(3, "Ballast Tank Level 4");
            pdm.AddInput(0, "Storage Comp Flood Level");
            pdm.AddInput(2, "Stbd Storage Compt Bilge Float Switch");
            pdm.AddInput(5, "Ballast Tank Level 2");
            pdm.AddInput(1, "Ballast Tank High Level");

            // Outputs
            pdm.AddOutput(6, "Window Defogger Fancoil");
            pdm.AddOutput(0, "Bilge Pump, Storage Comp Stbd");
            pdm.AddOutput(2, "MBMMR AN/PRC-117G 1/1");
            pdm.AddOutput(5, "MBMMR AN/PRC-117G 2/2");
            pdm.AddOutput(4, "MBMMR AN/PRC-117G 2/1");
            pdm.AddOutput(3, "MBMMR AN/PRC-117G 1/2");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 5A", 0x0b, channel);

            // Inputs
            pdm.AddInput(8, "Ballast Dewatering Pump Run Signal");
            pdm.AddInput(5, "Ballast Valve Open");
            pdm.AddInput(3, "Elex Cooling Compressor Binary Switch");
            pdm.AddInput(0, "Elex Cooling Fancoil Freeze Sensor");
            pdm.AddInput(2, "Radio Cabinet Thermoswitch");
            pdm.AddInput(6, "Ballast Valve Closed");
            pdm.AddInput(1, "Elex Cabinet Thermoswitch");

            // Outputs
            pdm.AddOutput(0, "IFF Transponder AN/APX-123");
            pdm.AddOutput(2, "Bilge Pump, Ballast Dewatering");
            pdm.AddOutput(5, "MBMMR AN/PRC-117G 3/2");
            pdm.AddOutput(4, "Stbd Circulation Fans");
            pdm.AddOutput(3, "MBMMR AN/PRC-117G 3/1");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 5B", 0x0c, channel);

            // Inputs
            pdm.AddInput(6, "TCS Control Master (KITS or SSN8 TCS)");
            pdm.AddInput(4, "Deck Lights Control");
            pdm.AddInput(3, "Overhead Strobe Light Control");
            pdm.AddInput(0, "Navigator Lights Control - Running");
            pdm.AddInput(2, "Follow-me Light Control");
            pdm.AddInput(5, "Darken Ship Control");
            pdm.AddInput(1, " Navigation Lights Control - Anchor");

            // Outputs
            pdm.AddOutput(5, "MBMMR AN/PRC-117G 4/2");
            pdm.AddOutput(0, "HF Xceiver AN/PRC-150");
            pdm.AddOutput(1, "MBMMR AN/PRC-117G 4/1");
            pdm.AddOutput(6, "Elex Cooling Fancoil");
            pdm.AddOutput(4, "AN/VIC3 MCS");
            pdm.AddOutput(3, "HUB No. 3 (COMMS)");
            pdm.AddOutput(2, "COMMS Display");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 2B", 0x04, channel);

            // Inputs
            pdm.AddInput(6, "Cockpit Defog System");
            pdm.AddInput(4, "Auxiliary Systems Control Master");
            pdm.AddInput(7, "Elex Cooling");
            pdm.AddInput(3, "Propulsion Control Master");
            pdm.AddInput(8, "Frame 14 Bilge Pump Run Signal");
            pdm.AddInput(0, "COMMS Control Master");
            pdm.AddInput(2, "NAV and CCFLIR Control Master");
            pdm.AddInput(5, "Bilge System Master");
            pdm.AddInput(1, "Weapons System Master");

            // Outputs
            pdm.AddOutput(0, "ENGR Display");
            pdm.AddOutput(1, "HUB No. 2");
            pdm.AddOutput(6, "CCFLIR");
            pdm.AddOutput(4, "Port Circulation Fans");
            pdm.AddOutput(2, "Bilge Pump, Frame 14");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 2C", 0x05, channel);

            // Inputs
            pdm.AddInput(11, "Stbd Engine Compt Bilge Pump Run Switch");
            pdm.AddInput(8, "Bustle Dewater Pump Port Run Switch");
            pdm.AddInput(6, "Bustle Dewater Pump Port Auto Switch");
            pdm.AddInput(9, "Bustle Dewater Pump Stbd Run Switch");
            pdm.AddInput(5, "Stbd Seachest Priming Pump Control");
            pdm.AddInput(10, "Port Engine Compt Bilge Pump Run Switch");
            pdm.AddInput(0, "Battery Parallel");
            pdm.AddInput(4, "Port Seachest Priming Pump Control");
            pdm.AddInput(7, "Bustle  Dewater Pump Stbd Auto Switch");
            pdm.AddInput(1, "Indicator Light Test");

            // Outputs
            pdm.AddOutput(0, "Helm Display");
            pdm.AddOutput(1, "Scanner Power");
            pdm.AddOutput(6, "Furuno MFDBB Processor");
            pdm.AddOutput(3, "ENTR Receiver");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 3B", 0x07, channel);

            // Inputs
            pdm.AddInput(10, "Port Frame 9 Bilge Pump Run Switch");
            pdm.AddInput(6, "Console Thermoswitch 1");
            pdm.AddInput(4, "Port Storage Compt Bilge Pump Auto Switch");
            pdm.AddInput(8, "Port Storage Compt Bilge Pump Run Switch");
            pdm.AddInput(11, "Stbd Frame 9 Bilge Pump Run Switch");
            pdm.AddInput(3, "Stbd Emergency Dewatering Control");
            pdm.AddInput(9, "Stbd Storage Compt Bilge Pump Run Switch");
            pdm.AddInput(0, "Compass Lighting Control");
            pdm.AddInput(2, "Port Emergency Dewatering Control");
            pdm.AddInput(5, "Stbd Storage Compt Bilge Pump Auto Switch");
            pdm.AddInput(1, "Console Thermoswitch 2");

            // Outputs
            pdm.AddOutput(5, "Ethernet Switch, Primary");
            pdm.AddOutput(0, "NAV Display");
            pdm.AddOutput(1, "Axis Video Server");
            pdm.AddOutput(6, "Rugged Power Unit #2");
            pdm.AddOutput(4, "Compass Lighting");
            pdm.AddOutput(3, "NDC-ASW NMEA Mux");
            pdm.AddOutput(2, "NDC-4 NMEA Mux");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 3C", 0x08, channel);

            // Inputs
            pdm.AddInput(9, "Stbd Frame 12 Bilge Pump Run Switch");
            pdm.AddInput(6, "Frame 14 Bilge Pump Auto Switch");
            pdm.AddInput(4, "Port Frame 12 Bilge Pump Auto Switch");
            pdm.AddInput(7, "Ballast Dewater Pump Auto Switch");
            pdm.AddInput(10, "Frame 14 Bilge Pump Run Switch");
            pdm.AddInput(3, "Stbd Frame 9 Bilge Pump Auto Switch");
            pdm.AddInput(8, "Port Frame 12 Bilge Pump Run Switch");
            pdm.AddInput(0, "Port Engine Compt Bilge Pump Auto Switch");
            pdm.AddInput(11, "Ballast Dewater Bilge Pump Run Switch");
            pdm.AddInput(2, "Port Frame 9 Bilge Pump Auto Switch");
            pdm.AddInput(5, "Stbd Frame 12 Bilge Pump Auto Switch");
            pdm.AddInput(1, "Stbd Engine Compt Bilge Pump Auto Switch");

            // Outputs
            pdm.AddOutput(5, "FLIR DBA");
            pdm.AddOutput(0, "CAN Bus Reader 1");
            pdm.AddOutput(1, "CAN Bus Reader 2");
            pdm.AddOutput(6, "Rugger Power Unit #1");
            pdm.AddOutput(4, "HUB No. 1");
            pdm.AddOutput(3, "Ethernet Switch Seconday");
            pdm.AddOutput(2, "CAN Bus Reader 3");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 24V 6A", 0x0d, channel);

            // Inputs
            pdm.AddInput(2, "Vector Drives Controller Summary Alarm");
            pdm.AddInput(3, "Vector Tabs Controller Summary Alarm");

            // Outputs
            pdm.AddOutput(5, "Vector Stbd Engine Controller");
            pdm.AddOutput(0, "Vector Drive Backup, Port");
            pdm.AddOutput(1, "Vector Drive Backup, Stbd");
            pdm.AddOutput(6, "Vector Hyd Dump Valve Controller");
            pdm.AddOutput(4, "Vector Port Engine Controller");
            pdm.AddOutput(3, "Vector Tabs Backup");
            pdm.AddOutput(2, "Vector Clutch Panel Backup");
            pdm.AddOutput(7, "Vector Clutch Panel Auto");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 12V 1A", 0x0e, channel);

            // Outputs
            pdm.AddOutput(0, "NMEA 2000 Network Power");
            pdm.AddOutput(1, "Receptacle, 12V");
            pdm.AddOutput(2, "Receptacles, 12V");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 12V 1B", 0x0f, channel);

            // Outputs
            pdm.AddOutput(6, "Receptacle 12V4");
            pdm.AddOutput(0, "Marine Band Radio");
            pdm.AddOutput(1, "Receptacle 12V1");
            pdm.AddOutput(5, "Searchlight Charger");
            pdm.AddOutput(2, "Receptacle 12V2");
            pdm.AddOutput(3, "Receptacle 12V3");

            PDMs.Add(pdm);

            pdm = new PDM("PDM 12V 1C", 0x10, channel);

            // Outputs
            pdm.AddOutput(0, "Receptacle, 12V");
            pdm.AddOutput(1, "Receptacles, 12V");
            pdm.AddOutput(2, "Generator Seawater Valve Power");

            PDMs.Add(pdm);

            foreach (var p in PDMs)
                p.AddVariables(Config);
        }


        protected override bool CheckShipID(MRADSShip ship)
        {
            var v = ship.State.GetValue(ship.ControlUnits[0], "MajorSWVersion");
            if (!v.Set)
                return false;

            // var vmu = ship.State.ProviderOnline(ship.Config.GetProvider("VMU Hull Console")) || ship.State.ProviderOnline(ship.Config.GetProvider("VMU ICE Console"));

            return v.GetValue<int>() == 67;
        }

        protected override DefaultBindVM GetViewModel(MRADSDataVM vmship, MRADSShipConfig config)
        {
            return new StandardShipVM(vmship, config);
        }

        protected override void InitClutchPanel(MRADSClutchPanel CP)
        {
            MessageBox.Show("need impl");
        }

        protected override void InitControlUnit(MRADSControlUnit CU)
        {
            MessageBox.Show("need impl");
        }
    }
}

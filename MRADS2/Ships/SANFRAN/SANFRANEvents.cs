using MRADS2;
using MRADS2.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRADS.Ships.SANFRAN
{
    class SANFRANEvents : StandardFaultEvent
    {
        public SANFRANEvents(MRADSShip ship) : base(ship)
        {
            AddFaults();
        }
        string FaultDescription(string varname)
        {
            int i;
            string nm = "";

            for (i = 0; i < varname.Length; i++)
            {
                if (i > 0 && char.IsUpper(varname[i]) && (!char.IsUpper(varname[i - 1]) || (i < (varname.Length - 1) && !char.IsUpper(varname[i + 1]))))
                    nm += " ";

                nm += varname[i];
            }
            return (nm);
        }
        void AddMainCUFault(string variable, string description = null)
        {
            if (description == null)
                description = FaultDescription(variable);

            AddFault(ship.ControlUnits[0].Name, variable, description);
        }
        void add_AM1_Faults(string variable, string description = null)
        {
            if (description == null)
                description = FaultDescription(variable);

            AddFault(ship.ControlUnits[1].Name, variable, description);
        }
        void AddClutchFault(string variable, string description = null)
        {
            if (description == null)
                description = FaultDescription(variable);

            AddFault(ship.ClutchPanels[0].Name, variable, description);
        }
        void AddFaults()
        {
            AddMainCUFault("PortEngineCommunicationTimeout");
            AddMainCUFault("StbdEngineCommunicationTimeout");
            AddMainCUFault("PortEngineControllerCommunicationTimeout");
            AddMainCUFault("StbdEngineControllerCommunicationTimeout");
            AddMainCUFault("MurphyLCDCommunicationTimeout");
            AddMainCUFault("CalibrationFault");
            AddMainCUFault("GeneralAlarm");
            AddMainCUFault("CANFault");
            //AddMainCUFault("DockmodeFault");
            //AddMainCUFault("ClutchCommunicationTimeout");
            //AddMainCUFault("StbdTrimNFUFault");
            //AddMainCUFault("StbdSteerNFUFault");
            //AddMainCUFault("PortTrimNFUFault");
            //AddMainCUFault("PortSteerNFUFault");
            //AddMainCUFault("StbdTrimSensorFault");
            //AddMainCUFault("StbdSteerSensorFault");
            //AddMainCUFault("PortTrimSensorFault");
            //AddMainCUFault("PortSteerSensorFault");
            //AddMainCUFault("PortLeverFault");
            //AddMainCUFault("StbdLeverFault");
            //AddMainCUFault("FwdPanelPortTrimJoystickFault");
            //AddMainCUFault("FwdPanelStbdTrimJoystickFault");
            //AddMainCUFault("HelmFault");
            //AddMainCUFault("TillerFault");
            //AddMainCUFault("JoyXFault");
            //AddMainCUFault("JoyYFault");
            //AddMainCUFault("NavigatorPortTrimJoystickFault");
            //AddMainCUFault("NavigatorStbdTrimJoystickFault");
            //AddMainCUFault("DockmodeFaultDueToSensorFault");
            //AddMainCUFault("DockmodeFaultDueToOFFTrollingSwitch");
            //AddMainCUFault("DockmodeFaultDueToLowRPM");
            //AddMainCUFault("DockmodeFaultDueToJoyXFault");
            //AddMainCUFault("DockmodeFaultDueToEngineCommunication");
            //AddMainCUFault("DockmodeFaultDueToEngineControllerCommunication");
            //AddMainCUFault("DockmodeFaultDueToClutchCommunication");
            //AddMainCUFault("DockmodeFaultDueToTillerFault");

            //add_AM1_Faults("DriveControlUnitCommunicationTimeout");
            //add_AM1_Faults("ClutchCommunicationTimeout");
            //add_AM1_Faults("PortEngineCommunicationTimeout");
            //add_AM1_Faults("StbdEngineCommunicationTimeout");
            //add_AM1_Faults("PortEngineControllerCommunicationTimeout");
            //add_AM1_Faults("StbdEngineControllerCommunicationTimeout");
            //add_AM1_Faults("PortTabNFUFault");
            //add_AM1_Faults("StbdTabNFUFault");
            //add_AM1_Faults("PortTabSensorFault");
            //add_AM1_Faults("StbdTabSensorFault");
            //add_AM1_Faults("FwdPanelTrimJoystickFault");
            //add_AM1_Faults("FwdPanelRollJoystickFault");
            //add_AM1_Faults("NavigatorStationTrimJoystickFault");
            //add_AM1_Faults("NavigatorStationRollJoystickFault");
            //add_AM1_Faults("TillerFault");
            //add_AM1_Faults("HelmFault");
            //add_AM1_Faults("CalibrationFault");
            //add_AM1_Faults("GeneralAlarm");
            //add_AM1_Faults("CANFault");

            AddClutchFault("DriveControlUnitCommunicationTimeout");
        }
    }
    public class SANFRANVariableChangedEvent : StandardVariableChangeEvent
    {
        const int EngineStartRPMThreshold = 100;
        public SANFRANVariableChangedEvent(MRADSShip ship) : base(ship)
        {
            InitEvents();
        }
        void InitEvents()
        {
            AddThresholdEvent("Port Engine", "EngineRPM", EngineStartRPMThreshold, "Port engine stopped", "Port engine started");
            AddThresholdEvent("Starboard Engine", "EngineRPM", EngineStartRPMThreshold, "Starboard engine stopped", "Starboard engine started");

            AddBoolEvent("Drive Control Unit", "DockingTransitState", "Entered transit mode", "Entered docking mode");

            AddEvent("Drive Control Unit", "StationInControl", true, (v, p, _) =>
            {
                var val = v.GetValue<int>();

                switch (val)
                {
                    case 0:
                        return ("Joystick + Tiller station is now in control");
                    case 1:
                        return ("Helm + Levers station is now in control");
                    default:
                        return ("No station is now in control");
                }
            });
        }

    }
    class SANFRANTripTopSpeedEvent : MRADSEventHandler
    {
        DateTime topspeedtime = DateTime.MinValue;
        double topspeed = 0;
        public SANFRANTripTopSpeedEvent(MRADSShip ship) : base(ship)
        {
        
        }
        public override IEnumerable<MRADSEvent> EventCheck(MRADSShipState state, MRADSDataImporter.MsgEntry msg, MRADSDataChangeSet changes)
        {
            if (changes.Provider.Name == "Murphy LCD")
            {
                var boatspeed = changes.Changes.SingleOrDefault(a => a.VariableDefinition.Name == "BoatSpeed");

                if (boatspeed.Set && boatspeed.GetValue<double>() > topspeed)
                {
                    topspeedtime = changes.Time;
                    topspeed = boatspeed.GetValue<double>();
                }
            }

            return (null);
        }

        public override MRADSEvent[] LastMessage(MRADSShipState state, MRADSDataImporter.MsgEntry msg)
        {
            return (new MRADSEvent[] { new MRADSEvent(topspeedtime, MRADSEvent.EventCategory.Status, $"Trip top speed reached {topspeed} kn", new MRADSEvent.VariableID("Murphy LCD", "BoatSpeed")) });
        }
    }

    class SANFRANMismatchEvent : MRADSEventHandler
    {
        const int EngineStartRPMThreshold = 100;
        const int EngineRPMDifferenceThreshold = 20;
        Dictionary<string, bool> mismatch = new Dictionary<string, bool>();

        public SANFRANMismatchEvent(MRADSShip ship) : base(ship)
        {
            mismatch["Port Engine"] = false;
            mismatch["Starboard Engine"] = false;
        }
        public override IEnumerable<MRADSEvent> EventCheck(MRADSShipState state, MRADSDataImporter.MsgEntry msg, MRADSDataChangeSet changes)
        {
            if ((changes.Provider.Name == "Port Engine" || changes.Provider.Name == "Starboard Engine") && changes.Changes.Any(a => a.VariableDefinition.Name == "EngineRPM" || a.VariableDefinition.Name == "DesiredRPM"))
            {
                var enginerpm = state.GetValue(changes.Provider, "EngineRPM").GetValue<int>();
                var desiredrpm = state.GetValue(changes.Provider, "DesiredRPM").GetValue<int>();

                if (enginerpm > EngineStartRPMThreshold && desiredrpm > EngineStartRPMThreshold && Math.Abs(enginerpm - desiredrpm) > EngineRPMDifferenceThreshold)
                {
                    if (!mismatch[changes.Provider.Name])
                    {
                        var ret = new MRADSEvent(changes.Time, MRADSEvent.EventCategory.Warning, $"Mismatched {changes.Provider.Name} command ({desiredrpm} RPM) and feedback ({enginerpm} RPM)",
                            new MRADSEvent.VariableID(changes.Provider.Name, "EngineRPM"), new MRADSEvent.VariableID(changes.Provider.Name, "DesiredRPM"));

                        mismatch[changes.Provider.Name] = true;

                        return (new MRADSEvent[] { ret });
                    }
                }
                else
                    mismatch[changes.Provider.Name] = false;
            }

            return (null);
        }
    }
}
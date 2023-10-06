using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRADS2.Ships.CCM
{
    class CCMFaultEvent : StandardFaultEvent
    {
        public CCMFaultEvent(MRADSShip ship) : base(ship)
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

        void AddDriveFault(string variable, string description = null)
        {
            if (description == null)
                description = FaultDescription(variable);

            AddFault(ship.ControlUnits[0].Name, variable, description);
        }

        void AddTabFault(string variable, string description = null)
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
            AddDriveFault("PortEngineCommunicationTimeout");
            AddDriveFault("StbdEngineCommunicationTimeout");
            AddDriveFault("PortEngineControllerCommunicationTimeout");
            AddDriveFault("StbdEngineControllerCommunicationTimeout");
            AddDriveFault("MurphyLCDCommunicationTimeout");
            AddDriveFault("CalibrationFault");
            AddDriveFault("GeneralAlarm");
            AddDriveFault("CANFault");
            AddDriveFault("DockmodeFault");
            AddDriveFault("ClutchCommunicationTimeout");
            AddDriveFault("StbdTrimNFUFault");
            AddDriveFault("StbdSteerNFUFault");
            AddDriveFault("PortTrimNFUFault");
            AddDriveFault("PortSteerNFUFault");
            AddDriveFault("StbdTrimSensorFault");
            AddDriveFault("StbdSteerSensorFault");
            AddDriveFault("PortTrimSensorFault");
            AddDriveFault("PortSteerSensorFault");
            AddDriveFault("PortLeverFault");
            AddDriveFault("StbdLeverFault");
            AddDriveFault("FwdPanelPortTrimJoystickFault");
            AddDriveFault("FwdPanelStbdTrimJoystickFault");
            AddDriveFault("HelmFault");
            AddDriveFault("TillerFault");
            AddDriveFault("JoyXFault");
            AddDriveFault("JoyYFault");
            AddDriveFault("NavigatorPortTrimJoystickFault");
            AddDriveFault("NavigatorStbdTrimJoystickFault");
            AddDriveFault("DockmodeFaultDueToSensorFault");
            AddDriveFault("DockmodeFaultDueToOFFTrollingSwitch");
            AddDriveFault("DockmodeFaultDueToLowRPM");
            AddDriveFault("DockmodeFaultDueToJoyXFault");
            AddDriveFault("DockmodeFaultDueToEngineCommunication");
            AddDriveFault("DockmodeFaultDueToEngineControllerCommunication");
            AddDriveFault("DockmodeFaultDueToClutchCommunication");
            AddDriveFault("DockmodeFaultDueToTillerFault");

            AddTabFault("DriveControlUnitCommunicationTimeout");
            AddTabFault("ClutchCommunicationTimeout");
            AddTabFault("PortEngineCommunicationTimeout");
            AddTabFault("StbdEngineCommunicationTimeout");
            AddTabFault("PortEngineControllerCommunicationTimeout");
            AddTabFault("StbdEngineControllerCommunicationTimeout");
            AddTabFault("PortTabNFUFault");
            AddTabFault("StbdTabNFUFault");
            AddTabFault("PortTabSensorFault");
            AddTabFault("StbdTabSensorFault");
            AddTabFault("FwdPanelTrimJoystickFault");
            AddTabFault("FwdPanelRollJoystickFault");
            AddTabFault("NavigatorStationTrimJoystickFault");
            AddTabFault("NavigatorStationRollJoystickFault");
            AddTabFault("TillerFault");
            AddTabFault("HelmFault");
            AddTabFault("CalibrationFault");
            AddTabFault("GeneralAlarm");
            AddTabFault("CANFault");

            AddClutchFault("DriveControlUnitCommunicationTimeout");
        }
    }

    public class CCMVariableChangeEvent : StandardVariableChangeEvent
    {
        const int EngineStartRPMThreshold = 100;

        public CCMVariableChangeEvent(MRADSShip ship) : base(ship)
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

    class CCMTripTopSpeedEvent : MRADSEventHandler
    {
        DateTime topspeedtime = DateTime.MinValue;
        double topspeed = 0;

        public CCMTripTopSpeedEvent(MRADSShip ship) : base(ship)
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

    class CCMRPMMismatchEvent : MRADSEventHandler
    {
        const int EngineStartRPMThreshold = 100;
        const int EngineRPMDifferenceThreshold = 20;
        Dictionary<string, bool> mismatch = new Dictionary<string, bool>();

        public CCMRPMMismatchEvent(MRADSShip ship) : base(ship)
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

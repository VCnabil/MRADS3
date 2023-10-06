using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRADS2.Ships;

namespace MRADS2.Ships.StandardShip
{
    /// <summary>
    /// Intended to implement standard events common across ships. This really needs to be updated to reflect
    /// "standard" better
    /// </summary>
    class StandardShipFaultEvent : StandardFaultEvent
    {
        public StandardShipFaultEvent(MRADSShip ship) : base(ship)
        {
            AddFaults();
        }

        void AddFaults()
        {
            int i;
            string[] stationmap = new string[] { "Bridge Center", "Port Wing", "Starboard Wing" };

            for (i = 1; i <= 3; i++)
            {
                AddCUFault($"Station{i}ThrustFault", $"{stationmap[i - 1]} Station Thrust Lever Fault");
                AddCUFault($"Station{i}AzimuthFault", $"{stationmap[i - 1]} Station Azimuth Lever Fault");
            }

            foreach (var cu in ship.ControlUnits)
            {
                foreach (var prefix in (string[])cu.Data)
                {
                    string nm = "";

                    for (i = 0; i < prefix.Length; i++)
                    {
                        if (i > 0 && char.IsUpper(prefix[i]))
                            nm += " ";

                        nm += prefix[i];
                    }

                    AddFault(cu.Name, prefix + "BucketFdbkFault", nm + " Bucket Feedback Sensor Fault");
                    AddFault(cu.Name, prefix + "NozzleFdbkFault", nm + " Nozzle Feedback Sensor Fault");

                    AddFault(cu.Name, prefix + "BucketNFUFault", nm + " Bucket Non-Follow-Up Fault");
                    AddFault(cu.Name, prefix + "NozzleNFUFault", nm + " Nozzle Non-Follow-Up Fault");
                }
            }

            AddCUFault("CalibrationFault", "Calibration Fault");
            AddCUFault("GeneralAlarm", "General Alarm");
            AddCUFault("ClutchPanelCommFault", "Clutch Panel Communication Fault");

            AddFault("Port Clutch Panel", "CANRxFault", "Control Unit Communication Fault");
            AddFault("Starboard Clutch Panel", "CANRxFault", "Control Unit Communication Fault");

            AddFault("LCD", "WJInterfaceModuleFault", "Waterjet Interface Module Communication Fault");
        }
    }

    public class StandardShipVariableChangeEvent : StandardVariableChangeEvent
    {
        const double VesselMovingThresholdHigh = 6;
        const double VesselMovingThresholdLow = 2;

        public StandardShipVariableChangeEvent(MRADSShip ship) : base(ship)
        {
            InitEvents();
        }

        void InitEvents()
        {
            AddCUThresholdEvent("CombinedSeparateAnalogInput", 2500,
                    "Combined / Separate switch toggled and now set to Combined Mode",
                    "Combined / Separate switch toggled and now set to Separate Mode");

            AddCUThresholdEvent("PortOrStbdLeverAnalogInput", 2500,
                    "Port / Starboard Lever Select switch toggled and now set to Port Lever",
                    "Port / Starboard Lever Select switch toggled and now set to Starboard Lever");

            AddEvent("GPS", "SpeedOverGround", false, (v, p, s) =>
            {
                double speed = v.GetValue<double>();
                bool moving = (s.State == null) ? false : (bool)s.State;

                if (!moving && speed > VesselMovingThresholdHigh)
                {
                    s.State = true;
                    return ("Vessel is moving");
                }
                else if (moving && speed < VesselMovingThresholdLow)
                {
                    s.State = false;
                    return ("Vessel has stopped");
                }
                else
                    return (null);
            });

            AddCUEvent("StationInControl", false, (v, p) =>
            {
                int val = v.GetValue<int>();

                if (p == null)
                    return ($"Initially {ControlUnitVM.GetStationInControlName(val)} is in control");

                return ($"After a station transfer {ControlUnitVM.GetStationInControlName(val)} is in control");
            });

            AddCUEvent("MessageCounter", true, (v, p) =>
            {
                byte val = (byte)v.GetValue<int>(), pval = (byte)p.GetValue<int>();

                if ((byte)(pval + 1) != val)
                    return ($"Heartbeat out-of-sync ({val} vs {pval})");
                else
                    return (null);

            }, MRADSEvent.EventCategory.Warning);

            foreach (var side in new string[] { "Port", "Starboard" })
            {
                foreach (var loc in new string[] { "Inboard", "Outboard" })
                    AddBoolEvent("LCD", $"BackupAutoState{side}{loc}", $"{side} {loc} Jet in Backup", $"{side} {loc} Jet in Auto");
            }

            foreach (var p in ship.ControlUnits[0].Parameters)
                AddCUEvent(p.Name, true, (v, p) => $"Parameter {v.VariableDefinition.Name} changed from {p.GetValue<int>()} to {v.GetValue<int>()}");
        }
    }

    public class StandardShipCalibrationEvent : MRADSEventHandler
    {
        static readonly string[] calcmdvars = new string[] { "CalibrationCmdDestination", "CalibrationCmdType", "CalibrationCmdOperation" };
        static readonly string[] calfdbkvars = new string[] { "CalibrationFdbkType", "CalibrationFdbkOperation" };

        public StandardShipCalibrationEvent(MRADSShip ship) : base(ship)
        {
        }

        public override IEnumerable<MRADSEvent> EventCheck(MRADSShipState state, MRADSDataImporter.MsgEntry msg, MRADSDataChangeSet changes)
        {
            if (changes.Provider.Name == "LCD" && changes.Changes.Select(a => a.VariableDefinition.Name).Intersect(calcmdvars).Count() > 0)
            {
                Dictionary<string, int> calcmdvalues = new Dictionary<string, int>();

                foreach (var s in calcmdvars)
                    calcmdvalues[s] = state.GetValue(changes.Provider, s).GetValue<int>();

                var cmdstring = LCDVM.GetCalibrationCommand(calcmdvalues["CalibrationCmdOperation"], calcmdvalues["CalibrationCmdDestination"], calcmdvalues["CalibrationCmdType"]);

                return (new MRADSEvent[] { new MRADSEvent(changes.Time, MRADSEvent.EventCategory.Status, cmdstring) });
            }
            else if (changes.Provider is MRADSControlUnit && changes.Changes.Select(a => a.VariableDefinition.Name).Intersect(calfdbkvars).Count() > 0)
            {
                Dictionary<string, int> calfdbkvalues = new Dictionary<string, int>();

                foreach (var s in calfdbkvars)
                    calfdbkvalues[s] = state.GetValue(changes.Provider, s).GetValue<int>();

                var fdbkstring = ControlUnitVM.GetCalibrationFeedback(calfdbkvalues["CalibrationFdbkType"], calfdbkvalues["CalibrationFdbkOperation"]);
                if (fdbkstring == null)
                    return (null);

                return (new MRADSEvent[] { new MRADSEvent(changes.Time, MRADSEvent.EventCategory.Status, $"{changes.Provider.Name}: {fdbkstring}") });
            }

            return (null);
        }
    }
}
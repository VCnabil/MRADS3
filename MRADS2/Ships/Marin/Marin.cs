using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRADS2.Ships
{
    class Marin : StandardShip.StandardShip
    {
        public Marin() : base()
        {
        }

        protected override void InitControlUnit(MRADSControlUnit CU)
        {
            PGNDecoder decoder;

            base.InitControlUnit(CU);

            decoder = CU.AddPGN(0xff71);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Sta1PortThrottleAnalogInput", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Sta1StbdThrottleAnalogInput", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Sta1HelmAnalogInput", 4, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("Sta1ReqTransferSwitch", 6, 0, true));

            decoder = CU.AddPGN(0xff72);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Sta2PortThrottleAnalogInput", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Sta2StbdThrottleAnalogInput", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Sta2TillerAnalogInput", 4, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("Sta2ReqTransferSwitch", 6, 0, true));

            decoder = CU.AddPGN(0xff73);
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Sta3PortThrottleAnalogInput", 0, 1));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Sta3StbdThrottleAnalogInput", 2, 3));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateInt("Sta3TillerAnalogInput", 4, 5));
            decoder.AddVariableDefinition(MRADSVariableDefinition.CreateBool("Sta3ReqTransferSwitch", 6, 0, true));
        }

        protected override bool CheckShipID(MRADSShip ship)
        {
            return (true);
        }
    }
}

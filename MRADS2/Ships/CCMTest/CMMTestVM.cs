using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRADS2.Ships.GenericViewModels;
using MRADS2.Ships.ViewModel;
using MRADS2.Ships.CCM;

namespace MRADS2.Ships.CCMTest
{
    public class CCMTestVM : CCMVM
    {
        public GPSVM GPS { get; }

        public VMUVM VMU { get; }

        public CCMTestVM(MRADSDataVM datavm, MRADSShipConfig config) : base(datavm, config, false)
        {
            GPS = new GPSVM();
            VMU = new VMUVM();
        }
    }
}

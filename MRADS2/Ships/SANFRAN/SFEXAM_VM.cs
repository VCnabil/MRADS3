using MRADS2.Ships.ViewModel;
using MRADS2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRADS.Ships.SANFRAN
{
    public class SFEXAM_VM : DataVMProvider, DefaultBindVM
    {
        public SFEXAM_VM(MRADSDataVM vmdata ,MRADSShipConfig config, bool oldvmu = true) : base(vmdata)
        {

        }
    }
}

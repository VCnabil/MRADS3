using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRADS2.Ships.ViewModel;

namespace MRADS2.Ships.GenericViewModels
{
    public class VintageVMUVM : DefaultBindVM
    {
        public BindVariable Pitch { get; private set; }
        public BindVariable Roll { get; private set; }
        public BindVariable Yaw { get; private set; }

        MRADSDataProvider VMU;

        public VintageVMUVM(MRADSDataProvider vmu)
        {
            VMU = vmu;
        }

        public void DefaultBind(MRADSDataVM datavm)
        {
            Pitch = datavm.GetVariable(VMU.Name, "Pitch").Bind();
            Roll = datavm.GetVariable(VMU.Name, "Roll").Bind();
            Yaw = datavm.GetVariable(VMU.Name, "Yaw").Bind();
        }
    }
}

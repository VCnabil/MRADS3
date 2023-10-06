using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRADS2.Ships.ViewModel
{
    // A common interface for ship view model classes to implement for binding to data view model variables
    public interface DefaultBindVM
    {
        public virtual void DefaultBind(MRADSDataVM datavm)
        {
        }
    }
}

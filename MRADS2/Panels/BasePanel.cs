using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MRADS2.Ships.ViewModel;

namespace MRADS2.Panels
{
    public abstract class BasePanel : UserControl
    {
        public MRADSDataVM VMData { get; private set; }

        public abstract string HeaderText { get; }

        public BasePanel(MRADSDataVM vmdata, DefaultBindVM vmship)
        {
            VMData = vmdata;

            DataContext = vmship;
        }

        public void Close()
        {
            DataContext = null;

            VMData = null;
        }
    }
}

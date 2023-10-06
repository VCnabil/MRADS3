using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MRADS2.Ships.ViewModel;

namespace MRADS2.Panels.StandardShip
{
    /// <summary>
    /// Interaction logic for ControlUnitInfoPanel.xaml
    /// </summary>
    public partial class ControlUnitInfoPanel : BasePanel
    {
        public ControlUnitInfoPanel(MRADSDataVM vmdata, DefaultBindVM vmship) : base(vmdata, vmship)
        {
            InitializeComponent();
        }

        public override string HeaderText => "Control Unit Info";
    }
}

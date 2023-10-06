using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for GPSPanel.xaml
    /// </summary>
    public partial class GPSPanel : BasePanel
    {
        public GPSPanel(MRADSDataVM vmdata, DefaultBindVM vmship) : base(vmdata, vmship)
        {
            InitializeComponent();
        }

        public override string HeaderText => "GPS";

        private void GPSShow_Click(object sender, RoutedEventArgs e)
        {
            double latitude, longitude;

            latitude = (double)VMData.GetVariable("GPS", "Latitude").RawValue;
            longitude = (double)VMData.GetVariable("GPS", "Longitude").RawValue;

            Process p = new Process();

            p.StartInfo.UseShellExecute = true;
            p.StartInfo.FileName = $"https://www.google.com/maps/place/{latitude},{longitude}";

            p.Start();
        }
    }
}

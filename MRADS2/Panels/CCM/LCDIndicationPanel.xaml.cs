using System;
using System.Collections.Generic;
using System.Globalization;
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
using MRADS2.Ships.CCM;
using MRADS2.Controls;

namespace MRADS2.Panels.CCM
{
    /// <summary>
    /// Interaction logic for VersionsPanel.xaml
    /// </summary>
    public partial class LCDIndicationPanel : BasePanel
    {
        public LCDIndicationPanel(MRADSDataVM vmdata, DefaultBindVM vmship) : base(vmdata, vmship)
        {
            InitializeComponent();
        }

        public override string HeaderText => "LCD Indication";
    }

    public class BarValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int)
                return ((double)(int)value);
            else if (value is double)
                return (value);
            else
                return (double.NaN);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("Not implemented");
        }
    }

    public class ClutchCommandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((string)value)
            {
                case "Ahead":
                    return ("AHD");
                case "Reverse":
                    return ("REV");
                case "Neutral":
                    return ("NEU");
                default:
                    return (value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("Not implemented");
        }
    }

    public class ClutchCommandColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Color c;

            switch ((string)value)
            {
                case "Ahead":
                    c = Colors.DarkGreen;
                    break;
                case "Reverse":
                    c = Colors.Red;
                    break;
                default:
                    c = Colors.Black;
                    break;
            }

            return (new SolidColorBrush(c));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("Not implemented");
        }
    }

    public class GaugeValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int)
                return (((1000 - (int)value) / 10.0).ToString("0.0"));
            else
                return (value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("Not implemented");
        }
    }
}

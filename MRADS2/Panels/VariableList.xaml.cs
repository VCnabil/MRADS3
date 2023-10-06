using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MRADS2.Panels
{
    /// <summary>
    /// Interaction logic for VariableList.xaml
    /// </summary>
    public partial class VariableList : BasePanel
    {
        public VariableList(MRADSDataVM vmdata) : base(vmdata, null)
        {
            InitializeComponent();

            DataContext = vmdata;
        }

        public override string HeaderText => "Raw Data";

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = (DataGridRow)sender;
            var value = (VariableVM)row.Item;

            if (!value.PlotState.Plotting)
                VMData.AddPlot(value);           
            else
                VMData.RemovePlot(value);
        }
    }

    public class PlotStateToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Brushes.Transparent;

            var plotstate = (VariableVM.PlottingState)value;

            if (!plotstate.Plotting)
                return (Brushes.White);
            else
                return (new SolidColorBrush(plotstate.PlotColor));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

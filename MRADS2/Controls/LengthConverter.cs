using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MRADS2.Controls
{
    public class LengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double actualWidth = (double)value;
            return   actualWidth * 2.1; // factor
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //public class TickConfig : INotifyPropertyChanged
    //{
    //    private double _myTickWidth;
    //    public double MyTickWidth
    //    {
    //        get { return _myTickWidth; }
    //        set
    //        {
    //            if (_myTickWidth != value)
    //            {
    //                _myTickWidth = value;
    //                OnPropertyChanged(nameof(MyTickWidth));
    //            }
    //        }
    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    protected virtual void OnPropertyChanged(string propertyName)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //    }

    //}
}

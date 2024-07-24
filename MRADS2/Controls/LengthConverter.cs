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
            return actualWidth * 0.01; // factor  1% of the width of the parent
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
 
}

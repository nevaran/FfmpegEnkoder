using System;
using System.Globalization;
using System.Windows.Data;

namespace FfmpegEnkoder.Converters
{
    class TwoThreadValuesToCheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if ((int)value == 0 || (int)value == (int)parameter)
                return "All";

            return $"{value}/{parameter}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

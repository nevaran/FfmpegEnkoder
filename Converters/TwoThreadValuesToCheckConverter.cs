using System;
using System.Globalization;
using System.Windows.Data;

namespace FfmpegEnkoder.Converters
{
    class TwoThreadValuesToCheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int threadCount = Environment.ProcessorCount;
            if ((int)value == 0 || (int)value == threadCount)
                return "All";

            return $"{value}/{threadCount}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

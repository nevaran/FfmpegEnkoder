using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace FfmpegEnkoder.Converters
{
    class ContainsArrayElementInArrayConverter : IMultiValueConverter
    {
        /// <summary>
        /// Includes two arguments: (0) Input of selected string, (1) List of string array
        /// </summary>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string selected = (string)values[0];
            string[] checkList = (string[])values[1];

            if (checkList.Contains(selected))
            {
                return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

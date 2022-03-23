using System;
using System.Globalization;
using System.Windows.Data;

namespace RoadCaptain.Runner.Converters
{
    public class BooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                if (parameter is string flip && flip == "invert")
                {
                    return !boolean;
                }

                return boolean;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                if (parameter is string flip && flip == "invert")
                {
                    return !boolean;
                }

                return boolean;
            }

            return value;
        }
    }
}

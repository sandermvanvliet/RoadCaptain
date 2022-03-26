using System;
using System.Globalization;
using System.Windows;
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

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}

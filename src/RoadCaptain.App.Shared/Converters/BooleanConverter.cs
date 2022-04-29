using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RoadCaptain.App.Shared.Converters
{
    public class BooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
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

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }
    }
}

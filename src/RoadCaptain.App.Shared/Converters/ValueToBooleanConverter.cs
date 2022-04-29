using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RoadCaptain.App.Shared.Converters
{
    public class ValueToBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }

            if (parameter == null)
            {
                return false;
            }

            return value.ToString().Equals(parameter as string, StringComparison.InvariantCultureIgnoreCase);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
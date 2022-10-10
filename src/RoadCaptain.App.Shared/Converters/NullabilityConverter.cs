// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RoadCaptain.App.Shared.Converters
{
    public class NullabilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                if ("invert".Equals(parameter))
                {
                    return true;
                }
                return false;
            }

            if ("invert".Equals(parameter))
            {
                return false;
            }

            return true;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }
    }
}


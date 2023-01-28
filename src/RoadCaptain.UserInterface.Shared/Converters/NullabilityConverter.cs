// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RoadCaptain.UserInterface.Shared.Converters
{
    public class NullabilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                if ("invert".Equals(parameter))
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }

            if ("invert".Equals(parameter))
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}


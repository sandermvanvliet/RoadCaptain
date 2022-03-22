﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RoadCaptain.Runner.Converters
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool visible)
            {
                if (parameter is string flip && flip == "invert")
                {
                    return !visible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }

                return visible
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
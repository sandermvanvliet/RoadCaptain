// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace RoadCaptain.App.Shared.Converters
{
    public class BitmapConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is string rawUri && targetType.IsAssignableFrom(typeof(Bitmap)))
            {
                Uri uri;

                // Allow for assembly overrides
                if (rawUri.StartsWith("avares://"))
                {
                    uri = new Uri(rawUri);
                }
                else
                {
                    var entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly == null)
                    {
                        return null;
                    }

                    var assemblyName = entryAssembly.GetName().Name;
                    uri = new Uri($"avares://{assemblyName}{rawUri}");
                }

                var asset = AssetLoader.Open(uri);

                return new Bitmap(asset);
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

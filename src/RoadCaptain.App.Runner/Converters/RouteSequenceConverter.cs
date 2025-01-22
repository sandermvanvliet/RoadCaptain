// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using RoadCaptain.App.Shared.Controls;

namespace RoadCaptain.App.Runner.Converters
{
    public class RouteSequenceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var routeSegmentSequence = value as IEnumerable<SegmentSequence>;

            if (targetType != typeof(List<RouteSegmentSequence>) || routeSegmentSequence == null)
            {
                throw new NotSupportedException();
            }

            return routeSegmentSequence
                .Select(x => new RouteSegmentSequence
                {
                    SegmentId = x.SegmentId,
                    Direction = x.Direction,
                    Type = x.Type
                })
                .ToList();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

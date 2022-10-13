using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared.Controls;

namespace RoadCaptain.App.RouteBuilder.Converters
{
    public class RouteSequenceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var routeSegmentSequence = value as IEnumerable<SegmentSequenceViewModel>;

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
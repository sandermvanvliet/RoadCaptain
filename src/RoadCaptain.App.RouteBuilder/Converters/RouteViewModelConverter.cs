using System;
using System.Globalization;
using Avalonia.Data.Converters;
using RoadCaptain.App.RouteBuilder.ViewModels;

namespace RoadCaptain.App.RouteBuilder.Converters
{
    internal class RouteViewModelConverter : IValueConverter
    {
        private static readonly Type PlannedRouteType = typeof(PlannedRoute);

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (targetType != PlannedRouteType)
            {
                throw new NotSupportedException();
            }
            
            var routeViewModel = value as RouteViewModel;

            return routeViewModel?.AsPlannedRoute();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

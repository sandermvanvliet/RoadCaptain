// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using RoadCaptain.App.Runner.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoadCaptain.App.Runner.Models
{
    public class RouteModel
    {
        public static RouteModel From(PlannedRoute? plannedRoute, List<Segment> segments, List<Segment> markers)
        {
            var model = new RouteModel
            {
                PlannedRoute = plannedRoute,
            };

            if (plannedRoute == null)
            {
                return model;
            }

            var metrics = plannedRoute
                .RouteSegmentSequence
                .Join(segments,
                    seq => seq.SegmentId,
                    segment => segment.Id,
                    (_, segment) => segment)
                .Select(segment => new
                {
                    segment.Distance,
                    segment.Ascent,
                    segment.Descent
                })
                .ToList();

            model.TotalDistance = Math.Round(metrics.Sum(s => s.Distance) / 1000, 1).ToString("0.0") + "km";
            model.TotalAscent = Math.Round(metrics.Sum(s => s.Ascent), 1).ToString("0.0") + "m";
            model.TotalDescent = Math.Round(metrics.Sum(s => s.Descent), 1).ToString("0.0") + "m";

            model.Markers = DetermineMarkersForRoute(plannedRoute, markers, segments);

            return model;
        }

        private static List<MarkerViewModel> DetermineMarkersForRoute(PlannedRoute plannedRoute, List<Segment> markers, List<Segment> segments)
        {
            var markersForRoute = new List<MarkerViewModel>();
            
            var routePoints = plannedRoute.GetTrackPoints(segments);

            // Determine bounding box of the route
            var routeBoundingBox = BoundingBox.From(routePoints);

            // Find markers that fall exactly inside the route bounding box
            var markersOnRoute = markers
                .Where(marker => routeBoundingBox.Contains(marker.BoundingBox))
                .ToList();

            foreach (var marker in markersOnRoute)
            {
                // For each marker try to follow the track
                // along the planned route from the starting
                // point of the marker. If it deviates more
                // than 25m at any point it doesn't match
                // with the route
                var fullMatch = true;

                foreach (var markerTrackPoint in marker.Points)
                {
                    var point = markerTrackPoint;

                    var closestOnRoute = routePoints
                        .Where(trackPoint => trackPoint.IsCloseTo(point))
                        .Select(trackPoint => new
                        {
                            TrackPoint = trackPoint,
                            Distance = trackPoint.DistanceTo(markerTrackPoint)
                        })
                        .MinBy(x => x.Distance);

                    if (closestOnRoute == null)
                    {
                        fullMatch = false;
                        break;
                    }

                    if (closestOnRoute.Distance > 25)
                    {
                        fullMatch = false;
                        break;
                    }
                }

                if (fullMatch)
                {
                    markersForRoute.Add(new MarkerViewModel(marker));
                }
            }

            return markersForRoute;
        }

        public PlannedRoute? PlannedRoute { get; private init; }

        public string? ZwiftRouteName => PlannedRoute?.ZwiftRouteName;

        public SportType Sport => PlannedRoute?.Sport ?? SportType.Unknown;

        public World? World => PlannedRoute?.World;
        public string? Name => PlannedRoute?.Name;
        public string TotalDistance { get; private set; }
        public string TotalAscent { get; private set; }
        public string TotalDescent { get; private set; }
        public bool IsLoop => PlannedRoute?.IsLoop ?? false;
        public List<MarkerViewModel> Markers { get; private set; } = new();
    }
}


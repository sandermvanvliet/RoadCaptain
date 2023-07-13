// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    internal class CalculatedElevationProfile
    {
        public double MinAltitude { get; }
        public double MaxAltitude { get; }
        public double TotalDistance { get; }
        public double AltitudeDelta { get; }
        public ImmutableList<ElevationGroup> ElevationGroups { get; }
        public ImmutableList<int> ElevationLines { get; }
        public ImmutableArray<TrackPoint> Points { get; }

        private CalculatedElevationProfile(IEnumerable<ElevationGroup> elevationGroups,
            List<TrackPoint> trackPoints,
            double minAltitude,
            double maxAltitude,
            double totalDistance)
        {
            MinAltitude = minAltitude;
            MaxAltitude = maxAltitude;
            TotalDistance = totalDistance;
            // When min is above sea level use max as the delta, otherwise include the min
            AltitudeDelta = minAltitude < 0 ? -minAltitude + maxAltitude : maxAltitude;
            ElevationGroups = elevationGroups.ToImmutableList();
            ElevationLines = CalculateElevationLines();
            Points = trackPoints.ToImmutableArray();// ElevationGroups.SelectMany(eg => eg.Points).ToImmutableArray();
        }

        private ImmutableList<int> CalculateElevationLines()
        {
            var elevationLines = new List<int> { 0 /* Always ensure sea-level exists */ };

            switch (AltitudeDelta)
            {
                case > 50 and < 250:
                {
                    const int altitudeStep = 50;
                    for (var altitude = altitudeStep; altitude <= MaxAltitude; altitude += altitudeStep)
                    {
                        elevationLines.Add(altitude);
                    }

                    break;
                }
                case > 100 and < 1000:
                {
                    const int altitudeStep = 100;
                    for (var altitude = altitudeStep; altitude <= MaxAltitude; altitude += altitudeStep)
                    {
                        elevationLines.Add(altitude);
                    }

                    break;
                }
                case > 250 and < 5000:
                {
                    const int altitudeStep = 250;
                    for (var altitude = altitudeStep; altitude <= MaxAltitude; altitude += altitudeStep)
                    {
                        elevationLines.Add(altitude);
                    }

                    break;
                }
            }

            return elevationLines.ToImmutableList();
        }

        private static CalculatedElevationProfile Empty => new(
            new List<ElevationGroup>(),
            new List<TrackPoint>(),
            0,
            0,
            0
        );

        private static int CalculateBucketedGrade(TrackPoint previousPoint, TrackPoint newPoint, double distanceFromLast)
        {
            var rawGrade = (Math.Abs(previousPoint.Altitude - newPoint.Altitude) / distanceFromLast) * 100;

            return rawGrade switch
            {
                > 0 and < 3 => 0,
                >= 3 and < 5 => 3,
                >= 5 and < 8 => 5,
                >= 8 and < 10 => 8,
                >= 10 => 10,
                _ => 0
            };
        }

        /// <summary>
        /// Get a list of all <see cref="TrackPoint"/>s that make up the route.
        /// </summary>
        /// <param name="route">The route to extract the TrackPoints from</param>
        /// <param name="segments">The list of segments to obtain the TrackPoints from</param>
        /// <remarks>This method takes into account the direction of the segment on the route so that the </remarks>
        private static List<TrackPoint> GetAllRoutePointsFromRoute(PlannedRoute route, List<Segment> segments)
        {
            var routePoints = new List<TrackPoint>();

            foreach (var routeStep in route.RouteSegmentSequence)
            {
                if (string.IsNullOrEmpty(routeStep.SegmentId))
                {
                    continue;
                }

                var segment = segments.SingleOrDefault(s => s.Id == routeStep.SegmentId);

                if (segment == null)
                {
                    throw new InvalidOperationException(
                        "Route contains a segment that I can't find and I can't continue with aggregating all track points of the route");
                }

                var points = segment.Points.ToArray();

                if (routeStep.Direction == SegmentDirection.BtoA)
                {
                    points = points.Reverse().ToArray();
                }

                routePoints.AddRange(points);
            }

            return routePoints;
        }

        internal static CalculatedElevationProfile From(PlannedRoute? route, List<Segment>? segments)
        {
            if (route == null || segments == null || !segments.Any() || !route.RouteSegmentSequence.Any())
            {
                return Empty;
            }

            // For our elevation profile we need the TrackPoints of the entire route
            var routePoints = GetAllRoutePointsFromRoute(route, segments);

            // And now for a bit of trickery.
            // To show an accurate plot of distance vs altitude we can't simply use the point index
            // as the x coordinate on the plot because the track points aren't consistently 1m apart.
            // What needs to happen is that we calculate the total distance of the route and use that
            // value to calculate how many pixels 1m is.
            // With that value we can then calculate the x coordinate based on the distance on segment
            // of a track point on the entire route. (Yes it's actually distance on route here but
            // we're creating new track points anyway so it doesn't matter... too much I hope)
            TrackPoint? previousPoint = null;
            double distanceOnRoute = 0;

            var trackPoints = new List<TrackPoint>();
            
            var elevationGroups = new List<ElevationGroup>();
            ElevationGroup? currentGroup = null;
            var overallIndex = 0;

            foreach (var point in routePoints)
            {
                var distanceFromLast = previousPoint == null
                    ? 0
                    : TrackPoint.GetDistanceFromLatLonInMeters(previousPoint.Latitude, previousPoint.Longitude, point.Latitude, point.Longitude);

                distanceOnRoute += distanceFromLast;

                var newPoint = new TrackPoint(point.Latitude, point.Longitude, point.Altitude, point.WorldId)
                {
                    DistanceFromLast = distanceFromLast,
                    DistanceOnSegment = distanceOnRoute,
                    Segment = point.Segment, // Copy 
                    Index = overallIndex++ // Recalculate this
                };

                var grade = previousPoint == null
                    ? -1
                    : CalculateBucketedGrade(previousPoint, newPoint, distanceFromLast);

                if (currentGroup == null)
                {
                    currentGroup = new ElevationGroup();
                    elevationGroups.Add(currentGroup);
                }
                else if (Math.Abs(currentGroup.Grade - (-1)) < 0.1)
                {
                    currentGroup.Grade = grade;
                }
                else if (Math.Abs(currentGroup.Grade - grade) > 0.1 && currentGroup.Points.Count > 1)
                {
                    var lastPointOfLastGroup = currentGroup.Points.Last();
                    currentGroup = new ElevationGroup
                    {
                        Grade = grade
                    };
                    currentGroup.Points.Add(lastPointOfLastGroup);
                    elevationGroups.Add(currentGroup);
                }

                currentGroup.Add(newPoint);
                trackPoints.Add(newPoint);

                previousPoint = point;
            }

            var minAltitude = trackPoints.Min(point => point.Altitude);
            var maxAltitude = trackPoints.Max(point => point.Altitude);
            
            return new CalculatedElevationProfile(
                elevationGroups,
                trackPoints,
                minAltitude, 
                maxAltitude, 
                trackPoints[^1].DistanceOnSegment /* we can cheat here as we've already calculated it above */);
        }

        public void CalculatePathsForElevationGroups(RenderParameters renderParameters)
        {
            foreach (var group in ElevationGroups)
            {
                var path = new SKPath();
                var points = group
                    .Points
                    .Select(point => new SKPoint(
                        (float)(point.DistanceOnSegment / renderParameters.MetersPerPixel), 
                        renderParameters.PlotHeight - renderParameters.CalculateYFromAltitude(point.Altitude)))
                    .ToList();

                points.Insert(0, new SKPoint(points[0].X, renderParameters.PlotHeight));
                points.Add(new SKPoint(points.Last().X, renderParameters.PlotHeight));
                path.AddPoly(points.ToArray());
                group.Path = path;
            }
        }

        public TrackPoint? GetClosestPointOnRoute(TrackPoint trackPoint)
        {
            return Points
                .Where(point => point.IsCloseTo(trackPoint))
                .Select(point => new
                {
                    Point = point,
                    Distance = TrackPoint.GetDistanceFromLatLonInMeters(point.Latitude, point.Longitude,
                        trackPoint.Latitude, trackPoint.Longitude)
                })
                .MinBy(x => x.Distance)
                ?.Point;
        }
    }
}

// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;

namespace RoadCaptain.App.Shared.Controls
{
    internal class RenderParameters
    {
        private const float Padding = 40f;
        private const int ZoomToSegmentOffset = 10;
        public const int MetersToShowInMovingWindow = 500;
        public double MetersPerPixel { get; }
        private float AltitudeOffset { get; }
        private float AltitudeScaleFactor { get; }
        public float PlotHeight { get; }
        public Rect TotalPlotBounds { get; }
        public float TranslateX { get; }

        private RenderParameters(
            double metersPerPixel, 
            double totalDistance, 
            double minAltitude, 
            double altitudeDelta, 
            Rect viewBounds,
            double startDistanceOnRoute)
        {
            MetersPerPixel = metersPerPixel;
            var plotWidth = Math.Round(totalDistance / metersPerPixel, 0, MidpointRounding.AwayFromZero);
            PlotHeight = (float)viewBounds.Height;
            TotalPlotBounds = new Rect(0, 0,  plotWidth, PlotHeight);
            AltitudeOffset = (float)(minAltitude < 0 ? -minAltitude : 0);
            AltitudeScaleFactor = (float)((viewBounds.Height - (2 * Padding)) / altitudeDelta);
            TranslateX = (float)-Math.Round(startDistanceOnRoute / metersPerPixel, 0, MidpointRounding.AwayFromZero);
        }

        internal static RenderParameters From(RenderMode renderMode,
            Rect bounds,
            CalculatedElevationProfile? elevationProfile,
            TrackPoint? riderPosition, 
            List<Segment>? markers)
        {
            if (elevationProfile == null)
            {
                return new RenderParameters(1, 1, 1, 1, bounds, 0);
            }
            
            // Depending on which mode we want:
            // - All: Based on the total length of the route and the width of the screen, calculate how many meters 1 pixel is
            // - Moving: Based on the width of the screen, calculate how many meters 1 pixel is when that width means 500m. The elevation plot moves with the rider but only ever shows 500m in the viewport
            // - Segment: Based on the width of the screen and the length of the segment, calculate how many meters 1 pixel is. The elevation plot shows only the segment, the rider position moves along the segment
            // 
            // The 'Segment' mode is split into:
            // - AllSegment
            // - MovingSegment
            // This is to facilitate a fallback for when the rider is not on a segment.

            var parameters = renderMode switch
            {
                RenderMode.All => CalculateParametersForModeAll(elevationProfile, bounds),
                RenderMode.Moving => CalculateParametersForModeMoving(bounds, elevationProfile, riderPosition),
                RenderMode.MovingSegment =>
                    CalculateParametersForModeSegment(bounds, riderPosition, markers, elevationProfile) ??
                    CalculateParametersForModeMoving(bounds, elevationProfile, riderPosition),
                RenderMode.AllSegment => CalculateParametersForModeSegment(bounds, riderPosition, markers, elevationProfile) ??
                                         CalculateParametersForModeAll(elevationProfile, bounds),
                _ => throw new InvalidOperationException("Invalid render mode, can't figure out what to display")
            };

            return parameters;
        }

        private static RenderParameters? CalculateParametersForModeSegment(
            Rect bounds, 
            TrackPoint? riderPosition,
            IReadOnlyCollection<Segment>? markers, 
            CalculatedElevationProfile elevationProfile)
        {
            // If there is no known position then the rider definitely is not on a segment...
            if (riderPosition == null)
            {
                return null;
            }
            
            // If there are no climb segments then the rider definitely is not on a segment...
            if (markers == null)
            {
                return null;
            }
            
            var segmentContainingRiderPosition = markers.SingleOrDefault(m => m.Contains(riderPosition));

            if (segmentContainingRiderPosition == null)
            {
                return null;
            }

            var distance = segmentContainingRiderPosition.Distance + (2 * ZoomToSegmentOffset);

            return new RenderParameters(
                MetersPerPixelFrom(distance, bounds.Width), 
                elevationProfile.TotalDistance, 
                elevationProfile.MinAltitude, 
                elevationProfile.AltitudeDelta, 
                bounds,
                elevationProfile.GetClosestPointOnRoute(segmentContainingRiderPosition.A)!.DistanceOnSegment - ZoomToSegmentOffset);
        }

        private static RenderParameters CalculateParametersForModeMoving(
            Rect bounds, 
            CalculatedElevationProfile elevationProfile,
            TrackPoint? riderPosition)
        {
            const int metersBefore = 20;

            var startDistanceOnRoute = 0d;

            if (riderPosition != null && !riderPosition.Equals(TrackPoint.Unknown))
            {
                var closest = elevationProfile.GetClosestPointOnRoute(riderPosition);

                var endClampStart = elevationProfile.TotalDistance - MetersToShowInMovingWindow;

                startDistanceOnRoute = closest!.DistanceOnSegment > endClampStart
                    ? endClampStart
                    : closest.DistanceOnSegment < metersBefore
                        ? 0
                        : closest.DistanceOnSegment - metersBefore;
            }

            return new RenderParameters(
                MetersPerPixelFrom(MetersToShowInMovingWindow, bounds.Width), 
                elevationProfile.TotalDistance, 
                elevationProfile.MinAltitude, 
                elevationProfile.AltitudeDelta, 
                bounds, 
                startDistanceOnRoute);
        }

        private static RenderParameters CalculateParametersForModeAll(CalculatedElevationProfile elevationProfile, Rect bounds)
        {
            return new RenderParameters(
                MetersPerPixelFrom(elevationProfile.TotalDistance, bounds.Width), 
                elevationProfile.TotalDistance, 
                elevationProfile.MinAltitude, 
                elevationProfile.AltitudeDelta, 
                bounds, 
                0);
        }

        public float CalculateYFromAltitude(double altitude)
        {
            return (float)((altitude + AltitudeOffset) * AltitudeScaleFactor) + Padding;
        }

        private static double MetersPerPixelFrom(double meters, double viewPortWidth)
        {
            return Math.Round(
                meters / viewPortWidth, 
                2,
                MidpointRounding.AwayFromZero);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    internal class RenderParameters
    {
        private float _padding = 40f;
        public double MetersPerPixel { get; }
        public double PlotWidth { get; }
        public float AltitudeOffset { get; }
        public float AltitudeScaleFactor { get; }
        public float PlotHeight { get; }

        private RenderParameters(double metersPerPixel, double totalDistance, double minAltitude, double altitudeDelta, Rect viewBounds)
        {
            MetersPerPixel = metersPerPixel;
            PlotWidth = totalDistance / metersPerPixel;
            PlotHeight = (float)viewBounds.Height;
            AltitudeOffset = (float)(minAltitude < 0 ? -minAltitude : 0);
            AltitudeScaleFactor = (float)((viewBounds.Height - (2 * _padding)) / altitudeDelta);
        }

        internal static RenderParameters From(RenderMode renderMode,
            Rect bounds,
            CalculatedElevationProfile elevationProfile,
            TrackPoint? riderPosition, 
            List<Segment>? markers)
        {
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
                RenderMode.Moving => CalculateParametersForModeMoving(bounds, elevationProfile),
                RenderMode.MovingSegment =>
                    CalculateParametersForModeSegment(bounds, riderPosition, markers, elevationProfile) ??
                    CalculateParametersForModeMoving(bounds, elevationProfile),
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

            var metersPerPixel = Math.Round(
                segmentContainingRiderPosition.Distance / bounds.Width, 
                0,
                MidpointRounding.AwayFromZero);

            return new RenderParameters(metersPerPixel, elevationProfile.TotalDistance, elevationProfile.MinAltitude, elevationProfile.AltitudeDelta, bounds);
        }

        private static RenderParameters CalculateParametersForModeMoving(Rect bounds, CalculatedElevationProfile elevationProfile)
        {
            const int metersToShowInMovingWindow = 500;
            
            var metersPerPixel = Math.Round(
                metersToShowInMovingWindow / bounds.Width, 0,
                MidpointRounding.AwayFromZero);

            return new RenderParameters(metersPerPixel, elevationProfile.TotalDistance, elevationProfile.MinAltitude, elevationProfile.AltitudeDelta, bounds);
        }

        private static RenderParameters CalculateParametersForModeAll(CalculatedElevationProfile elevationProfile, Rect bounds)
        {
            var metersPerPixel = Math.Round(elevationProfile.TotalDistance / bounds.Width, 0,
                MidpointRounding.AwayFromZero);

            return new RenderParameters(metersPerPixel, elevationProfile.TotalDistance, elevationProfile.MinAltitude, elevationProfile.AltitudeDelta, bounds);
        }

        public float CalculateYFromAltitude(double altitude)
        {
            return (float)((altitude + AltitudeOffset) * AltitudeScaleFactor) + _padding;
        }

        public bool IsInView(SKPoint trackPoint)
        {
            return false;
        }
    }
}
// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared.Controls;
using SkiaSharp;

namespace RoadCaptain.App.RouteBuilder.Controls
{
    public class ElevationProfileRenderOperation : ICustomDrawOperation
    {
        private static readonly SKColor CanvasBackgroundColor = SKColor.Parse("#FFFFFF");
        private SKBitmap? _bitmap;
        private RouteViewModel? _route;
        private SKPath? _elevationPath;
        private Rect _bounds;
        private float _altitudeOffset;
        private readonly int _padding = 10;
        private double _altitudeScaleFactor = 1;
        private readonly List<float> _elevationLines = new();
        private readonly SKFont _defaultFont = new(SKTypeface.Default);
        private List<TrackPoint>? _routePoints;
        private float _step;
        private int _previousIndex;
        private TrackPoint? _riderPosition;
        private List<ElevationGroup>? _elevationGroups;
        private const int CircleMarkerRadius = 10;

        public RouteViewModel? Route
        {
            get => _route;
            set
            {
                _route = value;
                CreateElevationProfile();
            }
        }

        public Rect Bounds
        {
            get => _bounds;
            set
            {
                if (_bounds == value) return;
                _bounds = value;
                InitializeBitmap();
                CreateElevationProfile();
            }
        }

        public List<Segment>? Segments { get; set; }

        public TrackPoint? RiderPosition
        {
            get => _riderPosition;
            set
            {
                if (TrackPoint.Equals(_riderPosition, value)) return;
                _riderPosition = value;
                if (_riderPosition == null)
                {
                    // Reset
                    _previousIndex = 0;
                }
            }
        }

        public bool ShowClimbs { get; set; }
        public List<Segment>? Markers { get; set; }

        public void Dispose()
        {
        }

        public bool HitTest(Point p)
        {
            return false;
        }

        public void Render(IDrawingContextImpl context)
        {
            var canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;

            if (canvas == null)
            {
                return;
            }

            canvas.Clear(CanvasBackgroundColor);

            if (_bitmap is { Width: > 0 })
            {
                // TODO: Something smart so that we only render when actually needed
                using (var mapCanvas = new SKCanvas(_bitmap))
                {
                    RenderCanvas(mapCanvas);
                }

                canvas.DrawBitmap(_bitmap, 0, 0);
            }
        }

        public bool Equals(ICustomDrawOperation? other)
        {
            if (other is ElevationProfileRenderOperation)
            {
                return true;
            }

            return false;
        }

        private void CreateElevationProfile()
        {
            if (Route == null || Segments == null || !Segments.Any() || !Route.Sequence.Any())
            {
                _elevationPath = null;

                return;
            }

            var routePoints = new List<TrackPoint>();

            foreach (var routeStep in Route.Sequence)
            {
                var segment = GetSegmentById(routeStep.SegmentId);

                if (segment == null)
                {
                    // TODO: yolo!
                    continue;
                }

                var points = segment.Points.ToArray();

                if (routeStep.Direction == SegmentDirection.BtoA)
                {
                    points = points.Reverse().ToArray();
                }

                routePoints.AddRange(points);
            }

            // And now for a bit of trickery.
            // To show an accurate plot of distance vs altitude we can't simply use the point index
            // as the x coordinate on the plot because the track points aren't consistently 1m apart.
            // What needs to happen is that we calculate the total distance of the route and use that
            // value to calculate how many pixels 1m is.
            // With that value we can then calculate the x coordinate based on the distance on segment
            // of a track point on the entire route. (Yes it's actually distance on route here but
            // we're creating new track points anyway so it doesn't matter... too much I hope)
            TrackPoint? previousPoint = null;
            double distanceOnSegment = 0;

            _routePoints = new List<TrackPoint>();
            
            _elevationGroups = new List<ElevationGroup>();
            ElevationGroup? currentGroup = null;

            foreach (var point in routePoints)
            {
                var distanceFromLast = previousPoint == null
                    ? 0
                    : TrackPoint.GetDistanceFromLatLonInMeters(previousPoint.Latitude, previousPoint.Longitude, point.Latitude, point.Longitude);

                distanceOnSegment += distanceFromLast;

                var newPoint = new TrackPoint(point.Latitude, point.Longitude, point.Altitude, point.WorldId)
                {
                    DistanceFromLast = distanceFromLast,
                    DistanceOnSegment = distanceOnSegment,
                    Segment = point.Segment, // Copy 
                    Index = point.Index // Copy
                };

                var grade = previousPoint == null
                    ? -1
                    : CalculateBucketedGrade(previousPoint, newPoint, distanceFromLast);

                if (currentGroup == null)
                {
                    currentGroup = new ElevationGroup();
                    _elevationGroups.Add(currentGroup);
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
                    _elevationGroups.Add(currentGroup);
                }

                currentGroup.Add(newPoint);
                _routePoints.Add(newPoint);

                previousPoint = point;
            }

            var minAltitude = _routePoints.Min(point => point.Altitude);
            var maxAltitude = _routePoints.Max(point => point.Altitude);

            // When min is above sea level use max as the delta, otherwise include the min
            var altitudeDelta = minAltitude < 0 ? -minAltitude + maxAltitude : maxAltitude;

            _altitudeScaleFactor = (Bounds.Height - (2 * _padding)) / altitudeDelta;

            // When min is below sea level we need to correct the resulting coordinate
            // so it isn't rendered off-screen.
            _altitudeOffset = (float)(minAltitude < 0 ? -minAltitude : 0);

            // This works because we've calculated it above
            var totalDistanceMeters = Math.Round(_routePoints.Last().DistanceOnSegment, MidpointRounding.AwayFromZero);

            _step = (float)(Bounds.Width / totalDistanceMeters);

            var polyPoints = _routePoints
                .Select(point => new SKPoint((float)(_step * point.DistanceOnSegment), CalculateYFromAltitude(point.Altitude)))
                .ToArray();

            _elevationPath = new SKPath();
            _elevationPath.AddPoly(polyPoints, false);

            foreach (var group in _elevationGroups)
            {
                var path = new SKPath();
                var points = group
                    .Points
                    .Select(point => new SKPoint((float)(_step * point.DistanceOnSegment), CalculateYFromAltitude(point.Altitude)))
                    .ToList();

                points.Insert(0, new SKPoint(points[0].X, 0));
                points.Add(new SKPoint(points.Last().X, 0));
                path.AddPoly(points.ToArray());
                group.Path = path;
            }

            _elevationLines.Clear();
            _elevationLines.Add(0); // Always ensure sea-level exists

            if (altitudeDelta is > 50 and < 250)
            {
                var altitudeStep = 50;
                for (var altitude = altitudeStep; altitude < maxAltitude; altitude += altitudeStep)
                {
                    _elevationLines.Add(altitude);
                }
            }
            else if (altitudeDelta is > 100 and < 1000)
            {
                var altitudeStep = 100;
                for (var altitude = altitudeStep; altitude < maxAltitude; altitude += altitudeStep)
                {
                    _elevationLines.Add(altitude);
                }
            }
            else if (altitudeDelta is > 250 and < 5000)
            {
                var altitudeStep = 250;
                for (var altitude = altitudeStep; altitude < maxAltitude; altitude += altitudeStep)
                {
                    _elevationLines.Add(altitude);
                }
            }
        }

        private static double CalculateBucketedGrade(TrackPoint previousPoint, TrackPoint newPoint, double distanceFromLast)
        {
            var rawGrade = (Math.Abs(previousPoint.Altitude - newPoint.Altitude) / distanceFromLast) * 100;

            if(rawGrade is > 0 and < 3)
            {
                return 0;
            }
            if(rawGrade is >= 3 and < 5)
            {
                return 3;
            }
            if(rawGrade is >= 5 and < 8)
            {
                return 5;
            }
            if(rawGrade is >= 8 and < 10)
            {
                return 8;
            }
            if(rawGrade >= 10)
            {
                return 10;
            }

            return rawGrade;
        }

        private float CalculateYFromAltitude(double altitude)
        {
            return (float)((altitude + _altitudeOffset) * _altitudeScaleFactor) + _padding;
        }

        private Segment? GetSegmentById(string id)
        {
            return Segments?.SingleOrDefault(s => s.Id == id);
        }

        private void RenderCanvas(SKCanvas canvas)
        {
            canvas.Clear(CanvasBackgroundColor);

            if (_elevationGroups is { Count: > 0 })
            {
                // Flip the canvas because otherwise the elevation is upside down
                canvas.Save();
                canvas.Scale(1, -1);
                canvas.Translate(0, -(float)Bounds.Height);
                
                foreach (var group in _elevationGroups)
                {
                    canvas.DrawPath(group.Path, PaintForGrade(group.Grade));
                }

                if (RiderPosition != null && _routePoints != null)
                {
                    for (var index = _previousIndex; index < _routePoints.Count; index++)
                    {
                        if (_routePoints[index].Equals(RiderPosition))
                        {
                            DrawCircleMarker(
                                canvas,
                                new SKPoint(
                                    (float)(_step * _routePoints[index].DistanceOnSegment),
                                    CalculateYFromAltitude(RiderPosition.Altitude)),
                                SkiaPaints.RiderPositionFillPaint);

                            // RiderPosition always moves forward, so
                            // store this value and pick up from there
                            // on the next update.
                            _previousIndex = index;

                            break;
                        }
                    }
                }

                // Back to normal
                canvas.Restore();

                if (_routePoints != null && ShowClimbs && Markers!= null && Markers.Any())
                {
                    var climbMarkers = Markers.Where(m => m.Type == SegmentType.Climb).ToList();

                    var climbMarkersOnRoute = _routePoints
                        .Select(point => new
                        {
                            Point = point,
                            Marker = climbMarkers.FirstOrDefault(m => m.Contains(point))
                        })
                        .Where(x => x.Marker != null)
                        .GroupBy(x => x.Marker.Id, x => x.Marker, (key, values) => values.First())
                        .ToList();

                    foreach (var climbMarker in climbMarkersOnRoute)
                    {
                        var closestA = GetClosestPointOnRoute(climbMarker.A);
                        var closestB = GetClosestPointOnRoute(climbMarker.B);

                        if (closestA != null && closestB != null && closestA.DistanceOnSegment < closestB.DistanceOnSegment)
                        {
                            DrawClimbMarker(canvas, closestA, climbMarker.Name);
                            DrawClimbMarker(canvas, closestB, null);
                        }
                    }
                }
            }

            // Ensure sea-level exists
            if (!_elevationLines.Any())
            {
                _elevationLines.Add(0);
            }

            foreach (var elevation in _elevationLines)
            {
                var correctedAltitudeOffset = (float)(Bounds.Height - CalculateYFromAltitude(elevation));

                canvas.DrawLine(0, correctedAltitudeOffset, (float)Bounds.Width, correctedAltitudeOffset,
                    SkiaPaints.ElevationLinePaint);

                var text = elevation == 0 ? "Sea level" : elevation.ToString(CultureInfo.InvariantCulture) + "m";

                canvas.DrawText(text, 5, correctedAltitudeOffset, _defaultFont, SkiaPaints.ElevationLineTextPaint);
            }
        }

        private static SKPaint PaintForGrade(double grade)
        {
            switch (grade)
            {
                case 0:
                    return SkiaPaints.ElevationPlotGradeZeroPaint;
                case 3:
                    return SkiaPaints.ElevationPlotGradeThreePaint;
                case 5:
                    return SkiaPaints.ElevationPlotGradeFivePaint;
                case 8:
                    return SkiaPaints.ElevationPlotGradeEightPaint;
                case 10:
                    return SkiaPaints.ElevationPlotGradeTenPaint;
                default:
                    return SkiaPaints.ElevationPlotPaint;

            }
        }

        private void DrawClimbMarker(SKCanvas canvas, TrackPoint climbMarkerPoint, string? climbName)
        {
            var x = (float)(_step * climbMarkerPoint.DistanceOnSegment);
            canvas.DrawLine(
                x,
                0,
                x,
                (float)(Bounds.Height - _padding),
                SkiaPaints.ElevationPlotClimbSegmentPaint);

            if (climbName != null)
            {
                canvas
                    .DrawText(
                        climbName,
                        new SKPoint(
                            x + 4,
                            20),
                        SkiaPaints.ElevationPlotClimbTextPaint
                    );
            }
        }

        private TrackPoint? GetClosestPointOnRoute(TrackPoint climbMarkerPoint)
        {
            if (_routePoints == null)
            {
                return null;
            }

            return _routePoints
                .Where(point => point.IsCloseTo(climbMarkerPoint))
                .Select(point => new
                {
                    Point = point,
                    Distance = TrackPoint.GetDistanceFromLatLonInMeters(point.Latitude, point.Longitude,
                        climbMarkerPoint.Latitude, climbMarkerPoint.Longitude)
                })
                .MinBy(x => x.Distance)
                ?.Point;
        }

        private static void DrawCircleMarker(SKCanvas canvas, SKPoint point, SKPaint fill)
        {
            canvas.DrawCircle(point, CircleMarkerRadius, SkiaPaints.CircleMarkerPaint);
            canvas.DrawCircle(point, CircleMarkerRadius - SkiaPaints.CircleMarkerPaint.StrokeWidth, fill);
        }

        private void InitializeBitmap()
        {
            _bitmap = new SKBitmap((int)Bounds.Width, (int)Bounds.Height, SKColorType.Bgra8888, SKAlphaType.Premul);

            using var canvas = new SKCanvas(_bitmap);
        }
    }
}

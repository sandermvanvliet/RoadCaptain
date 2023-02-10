// Copyright (c) 2023 Sander van Vliet
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
using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    public class ElevationProfileRenderOperation : ICustomDrawOperation
    {
        private static readonly SKColor CanvasBackgroundColor = SKColor.Parse("#FFFFFF");
        private PlannedRoute? _route;
        private Rect _bounds;
        private float _altitudeOffset;
        private const float MarkerPadding = 30f;
        private readonly float _padding = 10f + MarkerPadding;
        private double _altitudeScaleFactor = 1;
        private readonly List<float> _elevationLines = new();
        private readonly SKFont _defaultFont = new(SKTypeface.Default);
        private List<TrackPoint>? _routePoints;
        private float _step;
        private int _previousIndex;
        private TrackPoint? _riderPosition;
        private List<ElevationGroup>? _elevationGroups;
        private readonly SKPaint _textPaint;
        private readonly SKPaint _fillPaint;
        private readonly SKPaint _circlePaint;
        private readonly SKFont _font;
        private readonly float _offsetX;
        private readonly float _offsetY;
        private readonly SKPaint _squarePaint;
        private readonly SKPaint _squarePaintAlternate;
        private readonly SKPaint _linePaint;
        private readonly SKPaint _finishCirclePaint;
        private readonly SKPaint _finishLinePaint;
        private readonly SKPaint _distanceLinePaint;
        private float? _zoomCenterStep;
        private bool _hasShifted;
        private const int CircleMarkerRadius = 10;
        private int _zoomWindowMetersAhead = 950;
        private int _zoomWindowMetersBehind = 50;

        public ElevationProfileRenderOperation() 
        {
            _textPaint = new SKPaint { Color = SKColor.Parse("#FFFFFF"), IsAntialias = true, Style = SKPaintStyle.Fill };
            _fillPaint = new SKPaint { Color = SKColor.Parse("#fc4119"), IsAntialias = true, Style = SKPaintStyle.Fill };
            _linePaint = new SKPaint { Color = SKColor.Parse("#fc4119"), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2, PathEffect = SKPathEffect.CreateDash(new [] { 4f, 2f}, 4) };
            _finishLinePaint = new SKPaint { Color = SKColor.Parse("#000000"), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2, PathEffect = SKPathEffect.CreateDash(new [] { 4f, 2f}, 4) };
            _circlePaint = new SKPaint { Color = SKColor.Parse("#FFFFFF"), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 3 };
            _finishCirclePaint = new SKPaint { Color = SKColor.Parse("#000000"), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 3 };
            _distanceLinePaint = new SKPaint { Color = SKColor.Parse("#999999"), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2, PathEffect = SKPathEffect.CreateDash(new [] { 4f, 2f}, 4)};
            
            _squarePaint = new SKPaint { Color = SKColor.Parse("#000000"), Style = SKPaintStyle.Fill };
            _squarePaintAlternate = new SKPaint { Color = SKColor.Parse("#FFFFFF"), Style = SKPaintStyle.Fill };

            _font = new SKFont { Size = 16, Embolden = true };
            var glyphs = _textPaint.GetGlyphs("K");
            _font.MeasureText(glyphs, out var textBounds);
            _offsetX = textBounds.Width / 2;
            _offsetY = textBounds.Height / 2;
        }

        public PlannedRoute? Route
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
        public bool ZoomOnCurrentPosition { get; set; }

        public int ZoomWindowDistance
        {
            get => _zoomWindowMetersBehind + _zoomWindowMetersAhead;
            set
            {
                var x = (int)Math.Round(value * 0.05, 0, MidpointRounding.AwayFromZero);

                _zoomWindowMetersBehind = x;
                _zoomWindowMetersAhead = value - x;
            }
        }

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

            if (Bounds is { Width: > 0 })
            {
               RenderCanvas(canvas);
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
            if (Route == null || Segments == null || !Segments.Any() || !Route.RouteSegmentSequence.Any())
            {
                _elevationGroups = null;

                return;
            }

            var routePoints = new List<TrackPoint>();

            foreach (var routeStep in Route.RouteSegmentSequence)
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

            if (ZoomOnCurrentPosition)
            {
                // If the route is less than the zoomed in viewport
                // use the route distance, otherwise use the zoomed in viewport distance.
                var viewPortMeters = Math.Min(
                    _zoomWindowMetersBehind+_zoomWindowMetersAhead,
                    totalDistanceMeters);

                _step = (float)(Bounds.Width / viewPortMeters);
                _zoomCenterStep = (float)(Bounds.Width / (_zoomWindowMetersBehind + _zoomWindowMetersAhead));
            }
            else
            {
                _step = (float)(Bounds.Width / totalDistanceMeters);
            }

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
                SKPoint? riderPositionPoint = null;

                var dx = 0f;

                if (RiderPosition != null && _routePoints != null)
                {
                    for (var index = _previousIndex; index < _routePoints.Count; index++)
                    {
                        if (_routePoints[index].Equals(RiderPosition))
                        {
                            riderPositionPoint = new SKPoint(
                                (float)(_step * _routePoints[index].DistanceOnSegment),
                                CalculateYFromAltitude(RiderPosition.Altitude));
                            
                            // RiderPosition always moves forward, so
                            // store this value and pick up from there
                            // on the next update.
                            _previousIndex = index;

                            break;
                        }
                    }
                }

                if (riderPositionPoint != null && riderPositionPoint.Value.X > _zoomWindowMetersBehind)
                {
                    _hasShifted = true;
                    dx = riderPositionPoint.Value.X - _zoomWindowMetersBehind;
                }
                else
                {
                    _hasShifted = false;
                }

                canvas.Translate(-dx, -(float)Bounds.Height);
                
                foreach (var group in _elevationGroups)
                {
                    canvas.DrawPath(group.Path, PaintForGrade(group.Grade));
                }

                if (riderPositionPoint != null)
                {
                    DrawCircleMarker(
                        canvas,
                        riderPositionPoint.Value,
                        SkiaPaints.RiderPositionFillPaint);
                }

                if (_routePoints != null && Markers!= null && Markers.Any())
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
                            DrawStartMarker(canvas, closestA);
                            DrawFinishFlag(canvas, closestB);
                        }
                    }
                }

                // Back to normal
                canvas.Restore();
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

            if (ZoomOnCurrentPosition)
            {
                for (var i = _hasShifted ? 100 : 0; i < _zoomWindowMetersAhead; i += 100)
                {
                    var x = _zoomWindowMetersBehind + (_zoomCenterStep.Value * i);
                    canvas.DrawLine(
                        x,
                        0,
                        x,
                        (float)(Bounds.Height),
                        _distanceLinePaint);

                    var text = (_hasShifted ? i : i + 100).ToString(CultureInfo.InvariantCulture) + "m";
                    
                    var correctedAltitudeOffset = (float)(Bounds.Height - CalculateYFromAltitude(0));
                    canvas.DrawText(text, x + 5, correctedAltitudeOffset, _defaultFont, SkiaPaints.ElevationLineTextPaint);
                }
            }
        }

        private void DrawStartMarker(SKCanvas canvas, TrackPoint climbMarkerPoint)
        {
            var radius = 12f;
            var x = (float)(_step * climbMarkerPoint.DistanceOnSegment);
            var y = 1.5f * radius;
            var startPoint = new SKPoint(x, y);
            canvas.DrawCircle(startPoint, radius, _fillPaint);
            canvas.DrawCircle(startPoint, radius, _circlePaint);

            canvas.DrawText("K", startPoint.X - _offsetX, startPoint.Y + _offsetY, _font, _textPaint);

            canvas.DrawLine(x, y + radius, x, (float)(Bounds.Height), _linePaint );
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

        private void DrawFinishFlag(SKCanvas canvas, TrackPoint climbMarkerPoint)
        {
            var finishFlagWidth = 12f;

            var x = (float)(_step * climbMarkerPoint.DistanceOnSegment);
            var y = 1.5f * finishFlagWidth;

            x -= (finishFlagWidth / 2);
            y -= (finishFlagWidth / 2);

            DrawFinishFlag(canvas, x, y, finishFlagWidth);

            canvas.DrawLine(x + finishFlagWidth / 2, y + (1.5f * finishFlagWidth), x + finishFlagWidth / 2, (float)(Bounds.Height), _finishLinePaint);
        }

        private void DrawFinishFlag(SKCanvas canvas, float x, float y, float width)
        {
            const int numberOfSquares = 4;
            var squareSize = width / numberOfSquares;

            var boundsMidX = width / 2;

            canvas.DrawCircle(x + boundsMidX, y + boundsMidX, boundsMidX + squareSize + _finishCirclePaint.StrokeWidth, _squarePaint);

            for (var row = 0; row < numberOfSquares; row++)
            {
                for (var index = 0; index < numberOfSquares; index++)
                {
                    canvas.DrawRect(
                        x + index * squareSize, 
                        y + row * squareSize, 
                        squareSize, 
                        squareSize,
                        index % 2 == row % 2 ? _squarePaint : _squarePaintAlternate);
                }
            }

            canvas.DrawCircle(x + boundsMidX, y + boundsMidX, boundsMidX + squareSize, _circlePaint);
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
    }
}

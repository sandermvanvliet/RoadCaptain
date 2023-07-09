// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

#nullable enable
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
    public class ElevationPlotLayeredRenderOperation: ICustomDrawOperation
    {
        private static readonly SKColor CanvasBackgroundColor = SKColor.Parse("#FFFFFF");
        private const int CircleMarkerRadius = 10;
        private Rect _bounds;
        private PlannedRoute? _route;
        private TrackPoint? _riderPosition;
        private int _previousIndex;
        private int _zoomWindowMetersBehind;
        private int _zoomWindowMetersAhead;
        private CalculatedElevationProfile? _elevationProfile;
        private RenderParameters? _renderParameters;
        private RenderMode _renderMode = RenderMode.All;
        private readonly SKFont _defaultFont = new(SKTypeface.Default);
        private readonly SKPaint[] _paintForGrade = {
            SkiaPaints.ElevationPlotGradeZeroPaint,
            SkiaPaints.ElevationPlotGradeZeroPaint,
            SkiaPaints.ElevationPlotGradeZeroPaint,
            SkiaPaints.ElevationPlotGradeThreePaint,
            SkiaPaints.ElevationPlotGradeThreePaint,
            SkiaPaints.ElevationPlotGradeFivePaint,
            SkiaPaints.ElevationPlotGradeFivePaint,
            SkiaPaints.ElevationPlotGradeFivePaint,
            SkiaPaints.ElevationPlotGradeEightPaint,
            SkiaPaints.ElevationPlotGradeEightPaint,
            SkiaPaints.ElevationPlotGradeTenPaint
        };
        private IEnumerable<Segment>? _climbMarkersOnRoute;
        private readonly SKPaint _finishLinePaint;
        private readonly SKPaint _circlePaint;
        private readonly SKPaint _finishCirclePaint;
        private readonly SKPaint _squarePaint;
        private readonly SKPaint _squarePaintAlternate;
        private readonly SKPaint _textPaint;
        private readonly SKPaint _fillPaint;
        private readonly SKPaint _linePaint;
        private readonly SKFont _font;
        private readonly float _komLetterOffsetX;
        private readonly float _komLetterOffsetY;
        private readonly SKPaint _distanceLinePaint;

        public ElevationPlotLayeredRenderOperation()
        {
            _textPaint = new SKPaint { Color = SKColor.Parse("#FFFFFF"), IsAntialias = true, Style = SKPaintStyle.Fill };
            _fillPaint = new SKPaint { Color = SKColor.Parse("#fc4119"), IsAntialias = true, Style = SKPaintStyle.Fill };
            _linePaint = new SKPaint { Color = SKColor.Parse("#fc4119"), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2, PathEffect = SKPathEffect.CreateDash(new [] { 4f, 2f}, 4) };
            _finishLinePaint = new SKPaint { Color = SKColor.Parse("#000000"), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2, PathEffect = SKPathEffect.CreateDash(new [] { 4f, 2f}, 4) };
            _circlePaint = new SKPaint { Color = SKColor.Parse("#FFFFFF"), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 3 };
            _finishCirclePaint = new SKPaint { Color = SKColor.Parse("#000000"), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 3 };
            _squarePaint = new SKPaint { Color = SKColor.Parse("#000000"), Style = SKPaintStyle.Fill };
            _squarePaintAlternate = new SKPaint { Color = SKColor.Parse("#FFFFFF"), Style = SKPaintStyle.Fill };
            _distanceLinePaint = new SKPaint { Color = SKColor.Parse("#999999"), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2, PathEffect = SKPathEffect.CreateDash(new [] { 4f, 2f}, 4)};
            
            _font = new SKFont { Size = 16, Embolden = true };
            var glyphs = _textPaint.GetGlyphs("K");
            _font.MeasureText(glyphs, out var textBounds);
            _komLetterOffsetX = (textBounds.Width / 2) + 1;
            _komLetterOffsetY = textBounds.Height / 2;
        }

        public PlannedRoute? Route
        {
            get => _route;
            set
            {
                _route = value;
                _elevationProfile = CalculatedElevationProfile.From(_route, Segments);
                _renderParameters = RenderParameters.From(RenderMode, Bounds, _elevationProfile, RiderPosition, Markers);
                _elevationProfile.CalculatePathsForElevationGroups(_renderParameters);
            }
        }

        public Rect Bounds
        {
            get => _bounds;
            set
            {
                if (_bounds == value) return;
                _bounds = value;
                _elevationProfile = CalculatedElevationProfile.From(Route, Segments);
                _renderParameters = RenderParameters.From(RenderMode, Bounds, _elevationProfile, RiderPosition, Markers);
                _elevationProfile.CalculatePathsForElevationGroups(_renderParameters);
            }
        }

        public RenderMode RenderMode
        {
            get => _renderMode;
            set
            {
                if (value == _renderMode)
                {
                    return;
                }
                
                _renderMode = value;
                
                _renderParameters = RenderParameters.From(RenderMode, Bounds, _elevationProfile, RiderPosition, Markers);
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

        public bool ZoomToClimb { get; set; }

        public void Dispose()
        {
            // Nop
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

            if (Bounds is not { Width: > 0 })
            {
                return;
            }

            if (_renderParameters == null || _elevationProfile == null)
            {
                return;
            }

            RenderElevationProfile(canvas, _renderParameters, _elevationProfile);
            RenderElevationLines(canvas, _renderParameters, _elevationProfile);
            RenderSegmentMarkers(canvas, _renderParameters, Markers, _elevationProfile);
            RenderRiderPosition(canvas, _renderParameters, _elevationProfile);
        }

        private void RenderElevationProfile(SKCanvas canvas, RenderParameters renderParameters, CalculatedElevationProfile elevationProfile)
        {
            // Flip the canvas because otherwise the elevation is upside down
            canvas.Save();
            canvas.Scale(1, -1);
            
            // Because we flipped, we also need to translate
            canvas.Translate(renderParameters.TranslateX, -renderParameters.PlotHeight);
            
            foreach (var group in elevationProfile.ElevationGroups)
            {
                canvas.DrawPath(group.Path, _paintForGrade[group.Grade]);
            }
            
            // Back to normal
            canvas.Restore();
        }
        
        private void RenderElevationLines(SKCanvas canvas, RenderParameters renderParameters, CalculatedElevationProfile elevationProfile)
        {
            // Render the elevation lines
            foreach (var elevation in elevationProfile.ElevationLines)
            {
                var correctedAltitudeOffset = (float)(Bounds.Height - renderParameters.CalculateYFromAltitude(elevation));

                canvas.DrawLine(
                    0,
                    correctedAltitudeOffset, 
                    (float)Bounds.Width,
                    correctedAltitudeOffset,
                    SkiaPaints.ElevationLinePaint);

                var text = elevation == 0 ? "Sea level" : elevation.ToString(CultureInfo.InvariantCulture) + "m";

                canvas.DrawText(text, 5, correctedAltitudeOffset, _defaultFont, SkiaPaints.ElevationLineTextPaint);
            }
        }
        
        private void RenderSegmentMarkers(
            SKCanvas canvas,
            RenderParameters renderParameters, 
            List<Segment>? markers,
            CalculatedElevationProfile elevationProfile)
        {
            if (markers == null || !markers.Any())
            {
                return;
            }
            
            // Flip the canvas because otherwise the elevation is upside down
            canvas.Save();
            canvas.Scale(1, -1);
            
            // Because we flipped, we also need to translate
            canvas.Translate(renderParameters.TranslateX, -renderParameters.PlotHeight);
            
            var climbMarkersOnRoute = _climbMarkersOnRoute;

            if (climbMarkersOnRoute == null)
            {
                var climbMarkers = markers.Where(m => m.Type == SegmentType.Climb).ToList();

                _climbMarkersOnRoute = climbMarkersOnRoute = elevationProfile
                    .Points
                    .Select(point => new
                    {
                        Point = point,
                        Marker = climbMarkers.FirstOrDefault(m => m.Contains(point))
                    })
                    .Where(x => x.Marker != null)
                    .GroupBy(x => x.Marker!.Id, x => x.Marker!, (_, values) => values.First())
                    .ToList();
            }

            foreach (var climbMarker in climbMarkersOnRoute)
            {
                var closestA = elevationProfile.GetClosestPointOnRoute(climbMarker.A);
                var closestB = elevationProfile.GetClosestPointOnRoute(climbMarker.B);

                if (closestA != null && closestB != null && closestA.DistanceOnSegment < closestB.DistanceOnSegment)
                {
                    DrawStartMarker(canvas, closestA, renderParameters);
                    DrawFinishFlag(canvas, closestB, renderParameters);
                }
            }
            
            // Back to normal
            canvas.Restore();
        }

        private void DrawFinishFlag(SKCanvas canvas, TrackPoint climbMarkerPoint, RenderParameters renderParameters)
        {
            var finishFlagWidth = 12f;

            var x = (float)(climbMarkerPoint.DistanceOnSegment / renderParameters.MetersPerPixel);
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

        private void DrawStartMarker(SKCanvas canvas, TrackPoint climbMarkerPoint, RenderParameters renderParameters)
        {
            const float radius = 12f;
            var x = (float)(climbMarkerPoint.DistanceOnSegment / renderParameters.MetersPerPixel);
            const float y = 1.5f * radius;
            var startPoint = new SKPoint(x, y);
            canvas.DrawCircle(startPoint, radius, _fillPaint);
            canvas.DrawCircle(startPoint, radius, _circlePaint);

            canvas.DrawText("K", startPoint.X - _komLetterOffsetX, startPoint.Y + _komLetterOffsetY, _font, _textPaint);

            canvas.DrawLine(x, y + radius, x, (float)(Bounds.Height), _linePaint );
        }

        private void RenderRiderPosition(SKCanvas canvas, RenderParameters renderParameters, CalculatedElevationProfile elevationProfile)
        {
            // Flip the canvas because otherwise the elevation is upside down
            canvas.Save();
            canvas.Scale(1, -1);
            
            // Because we flipped, we also need to translate
            canvas.Translate(renderParameters.TranslateX, -renderParameters.PlotHeight);
            
            SKPoint? riderPositionPoint = null;
            
            if (RiderPosition != null && elevationProfile.Points.Any())
            {
                for (var index = _previousIndex; index < elevationProfile.Points.Length; index++)
                {
                    if (!elevationProfile.Points[index].Equals(RiderPosition))
                    {
                        continue;
                    }
                    
                    riderPositionPoint = new SKPoint(
                        (float)(elevationProfile.Points[index].DistanceOnSegment / renderParameters.MetersPerPixel),
                        renderParameters.CalculateYFromAltitude(RiderPosition.Altitude));
                            
                    // RiderPosition always moves forward, so
                    // store this value and pick up from there
                    // on the next update.
                    _previousIndex = index;

                    break;
                }
                
                if (riderPositionPoint != null)
                {
                    DrawCircleMarker(
                        canvas,
                        riderPositionPoint.Value,
                        SkiaPaints.RiderPositionFillPaint);
                }
            }
            
            // Back to normal
            canvas.Restore();
            
            if (RenderMode == RenderMode.Moving && riderPositionPoint != null)
            {
                RenderDistanceLines(canvas, renderParameters, riderPositionPoint.Value);
            }
        }

        private static void DrawCircleMarker(SKCanvas canvas, SKPoint point, SKPaint fill)
        {
            canvas.DrawCircle(point, CircleMarkerRadius, SkiaPaints.CircleMarkerPaint);
            canvas.DrawCircle(point, CircleMarkerRadius - SkiaPaints.CircleMarkerPaint.StrokeWidth, fill);
        }

        private void RenderDistanceLines(
            SKCanvas canvas, 
            RenderParameters renderParameters,
            SKPoint riderPositionPoint)
        {
            canvas.Save();
            
            // Because we flipped, we also need to translate
            canvas.Translate(renderParameters.TranslateX, 0);


            var metersPerStep = 100;
            var step = metersPerStep / renderParameters.MetersPerPixel;
            var numberOfSteps = (int)Math.Round((double)RenderParameters.MetersToShowInMovingWindow / metersPerStep, 0, MidpointRounding.ToZero);
            for (var i = 1; i <= numberOfSteps; i++)
            {
                var x = riderPositionPoint.X + (i * step);
                
                canvas.DrawLine(
                    (float)x,
                    0,
                    (float)x,
                    (float)(Bounds.Height),
                    _distanceLinePaint);

                var text = (i * 100).ToString(CultureInfo.InvariantCulture) + "m";
                    
                var correctedAltitudeOffset = (float)(Bounds.Height - renderParameters.CalculateYFromAltitude(0));
                canvas.DrawText(text, (float)(x + 5), correctedAltitudeOffset, _defaultFont, SkiaPaints.ElevationLineTextPaint);
            }

            canvas.Restore();
        }

        public bool Equals(ICustomDrawOperation? other)
        {
            return false;
        }
    }

    public enum RenderMode  
    {
        Unknown = 0,
        All = 1,
        Moving = 2,
        MovingSegment = 3,
        AllSegment = 4
    }
}

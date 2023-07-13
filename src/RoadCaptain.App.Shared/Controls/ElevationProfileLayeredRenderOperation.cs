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
    public class ElevationProfileLayeredRenderOperation: ICustomDrawOperation
    {
        private static readonly SKColor CanvasBackgroundColor = SKColor.Parse("#FFFFFF");
        private const int CircleMarkerRadius = 10;
        private Rect _bounds;
        private PlannedRoute? _route;
        private TrackPoint? _riderPosition;
        private int _previousIndex;
        private CalculatedElevationProfile? _elevationProfile;
        private RenderParameters? _renderParameters;
        private RenderMode _renderMode = RenderMode.All;
        private readonly SKFont _defaultFont = new(SKTypeface.Default);
        private readonly SKPaint[] _paintForGrade = {
            SkiaPaints.ElevationProfileGradeZeroPaint,
            SkiaPaints.ElevationProfileGradeZeroPaint,
            SkiaPaints.ElevationProfileGradeZeroPaint,
            SkiaPaints.ElevationProfileGradeThreePaint,
            SkiaPaints.ElevationProfileGradeThreePaint,
            SkiaPaints.ElevationProfileGradeFivePaint,
            SkiaPaints.ElevationProfileGradeFivePaint,
            SkiaPaints.ElevationProfileGradeFivePaint,
            SkiaPaints.ElevationProfileGradeEightPaint,
            SkiaPaints.ElevationProfileGradeEightPaint,
            SkiaPaints.ElevationProfileGradeTenPaint
        };
        private List<(Segment Climb, TrackPoint Start, TrackPoint Finish)> _climbMarkersOnRoute = new();
        private readonly SKPaint _finishLinePaint;
        private readonly SKPaint _circlePaint;
        private readonly SKPaint _finishCirclePaint;
        private readonly SKPaint _squarePaint;
        private readonly SKPaint _squarePaintAlternate;
        private readonly SKPaint _textPaint;
        private readonly SKPaint _fillPaintClimb;
        private readonly SKPaint _fillPaintSprint;
        private readonly SKPaint _linePaint;
        private readonly SKFont _font;
        private readonly float _komLetterOffsetX;
        private readonly float _komLetterOffsetY;
        private readonly SKPaint _distanceLinePaint;

        public ElevationProfileLayeredRenderOperation()
        {
            _textPaint = new SKPaint { Color = SKColor.Parse("#FFFFFF"), IsAntialias = true, Style = SKPaintStyle.Fill };
            _fillPaintClimb = new SKPaint { Color = SKColor.Parse("#fc4119"), IsAntialias = true, Style = SKPaintStyle.Fill };
            _fillPaintSprint = new SKPaint { Color = SKColor.Parse("#56A91D"), IsAntialias = true, Style = SKPaintStyle.Fill };
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

                if (_route != null && Markers != null && Segments != null)
                {
                    _elevationProfile = CalculatedElevationProfile.From(_route, Segments);
                    _renderParameters = RenderParameters.From(RenderMode, Bounds, _elevationProfile, RiderPosition, Markers);
                    _elevationProfile.CalculatePathsForElevationGroups(_renderParameters);
                    _climbMarkersOnRoute = PlannedRoute.CalculateClimbMarkers(Markers, _elevationProfile.Points);
                }
                else
                {
                    // Reset everything
                    _elevationProfile = null;
                    _renderParameters = null;
                    _climbMarkersOnRoute = new List<(Segment Climb, TrackPoint Start, TrackPoint Finish)>();
                }
            }
        }

        public Rect Bounds
        {
            get => _bounds;
            set
            {
                if (_bounds == value) return;
                _bounds = value;
                if (_elevationProfile != null)
                {
                    _renderParameters = RenderParameters.From(RenderMode, Bounds, _elevationProfile, RiderPosition, Markers);
                    _elevationProfile.CalculatePathsForElevationGroups(_renderParameters);
                }
                else
                {
                    // Clear render parameters
                    _renderParameters = null;
                }
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

                if (_elevationProfile != null)
                {
                    _renderParameters = RenderParameters.From(RenderMode, Bounds, _elevationProfile, RiderPosition, Markers);
                    _elevationProfile.CalculatePathsForElevationGroups(_renderParameters);
                }
                else if (_route != null && Markers != null && Segments != null)
                {
                    _elevationProfile = CalculatedElevationProfile.From(_route, Segments);
                    _renderParameters = RenderParameters.From(RenderMode, Bounds, _elevationProfile, RiderPosition, Markers);
                    _elevationProfile.CalculatePathsForElevationGroups(_renderParameters);
                    _climbMarkersOnRoute = PlannedRoute.CalculateClimbMarkers(Markers, _elevationProfile.Points);
                }
            }
        }

        public List<Segment>? Segments { get; set; }
        
        public List<Segment>? Markers { get; set; }

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
                else if(_elevationProfile != null)
                {
                    _renderParameters =
                        RenderParameters.From(RenderMode, Bounds, _elevationProfile, RiderPosition, Markers);
                    _elevationProfile.CalculatePathsForElevationGroups(_renderParameters);
                }
            }
        }

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
            RenderSegmentMarkers(canvas, _renderParameters);
            RenderRiderPosition(canvas, _renderParameters, _elevationProfile);
        }

        private void RenderElevationProfile(SKCanvas canvas, RenderParameters renderParameters, CalculatedElevationProfile elevationProfile)
        {
            // Flip the canvas because otherwise the elevation is upside down
            canvas.Save();
            
            // Because we flipped, we also need to translate
            canvas.Translate(renderParameters.TranslateX, 0);
            
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
            RenderParameters renderParameters)
        {
            // Flip the canvas because otherwise the elevation is upside down
            canvas.Save();
            
            // Because we flipped, we also need to translate
            canvas.Translate(renderParameters.TranslateX, 0);

            foreach (var climbMarker in _climbMarkersOnRoute)
            {
                DrawStartMarker(canvas, climbMarker.Start, renderParameters, climbMarker.Climb.Type);
                DrawFinishFlag(canvas, climbMarker.Finish, renderParameters);
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

        private void DrawStartMarker(SKCanvas canvas, TrackPoint climbMarkerPoint, RenderParameters renderParameters,
            SegmentType segmentType)
        {
            const float radius = 12f;

            var markerText = segmentType == SegmentType.Climb
                ? "K"
                : "S";
            
            var fillPaint = segmentType == SegmentType.Climb
                ? _fillPaintClimb
                : _fillPaintSprint;

            var x = (float)(climbMarkerPoint.DistanceOnSegment / renderParameters.MetersPerPixel);
            const float y = 1.5f * radius;
            var startPoint = new SKPoint(x, y);
            canvas.DrawCircle(startPoint, radius, fillPaint);
            canvas.DrawCircle(startPoint, radius, _circlePaint);

            var komLetterOffsetX = _komLetterOffsetX - (segmentType == SegmentType.Climb ? 0 : 1);
            
            canvas.DrawText(markerText, startPoint.X - komLetterOffsetX, startPoint.Y + _komLetterOffsetY, _font, _textPaint);

            canvas.DrawLine(x, y + radius, x, (float)(Bounds.Height), _linePaint );
        }

        private void RenderRiderPosition(SKCanvas canvas, RenderParameters renderParameters, CalculatedElevationProfile elevationProfile)
        {
            // Flip the canvas because otherwise the elevation is upside down
            canvas.Save();
            
            // Because we flipped, we also need to translate
            canvas.Translate(renderParameters.TranslateX, 0);
            
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
                        renderParameters.PlotHeight - renderParameters.CalculateYFromAltitude(RiderPosition.Altitude));
                            
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

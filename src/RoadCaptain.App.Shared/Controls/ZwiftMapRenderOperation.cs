// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace RoadCaptain.App.Shared.Controls
{
    internal class ZwiftMapRenderOperation : ICustomDrawOperation
    {
        private const int KomMarkerHeight = 32;
        private const int KomMarkerWidth = 6;
        private const int KomMarkerCenterX = 3;
        private const int KomMarkerCenterY = 16;
        private const int CircleMarkerRadius = 8;
        private static readonly SKColor CanvasBackgroundColor = SKColor.Parse("#FFFFFF");
        private SKBitmap? _bitmap;
        private Rect _bounds;
        private SKImage? _worldImage;
        private ZwiftWorldId _worldImageId;
        public SKMatrix LogicalMatrix { get; private set; }
        public string? HighlightedSegmentId { get; set; }
        public string? SelectedSegmentId { get; set; }
        public string? HighlightedMarkerId { get; set; }
        public Point Pan { get; set; } = new(0, 0);
        public float ZoomLevel { get; set; } = 1;
        public Point ZoomCenter { get; set; } = new(0, 0);
        public bool ShowSprints { get; set; }
        public bool ShowClimbs { get; set; }
        public SKPoint? RiderPosition { get; set; }
        public Dictionary<string, SKPath> SegmentPaths { get; set; } = new();
        public Dictionary<string, Marker> Markers { get; set; } = new();
        public SKPath RoutePath { get; set; } = new();

        public void Dispose()
        {
        }

        public Rect Bounds
        {
            get => _bounds;
            set
            {
                _bounds = value;

                InitializeBitmap();
            }
        }

        public float ZwiftMapScaleX { get; set; }
        public float ZwiftMapScaleY { get; set; }
        public float ZwiftMapTranslateX { get; set; }
        public float ZwiftMapTranslateY { get; set; }


        public bool HitTest(Point p)
        {
            return false;
        }

        public bool Equals(ICustomDrawOperation? other)
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

        private void RenderCanvas(SKCanvas canvas)
        {
            canvas.Clear(CanvasBackgroundColor);

            canvas.Save();

            ScaleAndTranslate(canvas);

            if (World != null)
            {
                RenderZwiftMap(canvas, World);
            }

            // Lowest layer are the segments
            foreach (var (segmentId, skiaPath) in SegmentPaths)
            {
                SKPaint segmentPaint;
                var isRouteSegment = false;

                // Use a different color for the selected segment
                if (segmentId == SelectedSegmentId)
                {
                    segmentPaint = SkiaPaints.SelectedSegmentPathPaint;
                }
                else if (segmentId == HighlightedSegmentId)
                {
                    segmentPaint = SkiaPaints.SegmentHighlightPaint;
                }
                else if (Sequence != null && !Sequence.Any() && IsSpawnPointSegment(segmentId))
                {
                    segmentPaint = SkiaPaints.SpawnPointSegmentPathPaint;
                }
                else if (Sequence != null && Sequence.Any(s => s.SegmentId == segmentId && s.IsLeadIn))
                {
                    isRouteSegment = true;
                    segmentPaint = SkiaPaints.LeadInPaint;
                }
                else if (Sequence != null && Sequence.Any(s => s.SegmentId == segmentId))
                {
                    isRouteSegment = true;
                    segmentPaint = SkiaPaints.RoutePathPaint;
                }
                else
                {
                    segmentPaint = SkiaPaints.SegmentPathPaint;
                }

                if ((OnlyShowRoute && isRouteSegment) || !OnlyShowRoute)
                {
                    canvas.DrawPath(skiaPath, segmentPaint);
                }
            }

            if (ShowClimbs || ShowSprints)
            {
                var drawnMarkers = new List<TrackPoint>();

                foreach (var (_, marker) in Markers)
                {
                    if (marker.Type == SegmentType.Climb && ShowClimbs)
                    {
                        var climbSegmentPaint = SkiaPaints.ClimbSegmentPaint;

                        if (HighlightedMarkerId == marker.Id)
                        {
                            climbSegmentPaint = SkiaPaints.MarkerHighlightPaint;
                        }

                        canvas.DrawPath(marker.Path, climbSegmentPaint);

                        using (new SKAutoCanvasRestore(canvas))
                        {
                            DrawClimbMarker(canvas, SkiaPaints.MarkerSegmentStartPaint, marker.StartAngle,
                                marker.StartDrawPoint);
                        }

                        // There are KOMs that end at the same(-ish) location which
                        // would cause the finish line to be drawn on top of each other.
                        // This prevents that.
                        if (!drawnMarkers.Any(kv => kv.IsCloseTo(marker.EndPoint)))
                        {
                            drawnMarkers.Add(marker.EndPoint);

                            using (new SKAutoCanvasRestore(canvas))
                            {
                                DrawClimbMarker(canvas, SkiaPaints.MarkerSegmentEndPaint, marker.EndAngle,
                                    marker.EndDrawPoint);
                            }
                        }
                    }
                    else if (marker.Type == SegmentType.Sprint && ShowSprints)
                    {
                        var sprintSegmentPaint = SkiaPaints.SprintSegmentPaint;

                        if (HighlightedMarkerId == marker.Id)
                        {
                            sprintSegmentPaint = SkiaPaints.MarkerHighlightPaint;
                        }

                        canvas.DrawPath(marker.Path, sprintSegmentPaint);

                        using (new SKAutoCanvasRestore(canvas))
                        {
                            DrawClimbMarker(canvas, SkiaPaints.MarkerSegmentStartPaint, marker.StartAngle,
                                marker.StartDrawPoint);
                        }

                        // There are KOMs that end at the same(-ish) location which
                        // would cause the finish line to be drawn on top of each other.
                        // This prevents that.
                        if (!drawnMarkers.Any(kv => kv.IsCloseTo(marker.EndPoint)))
                        {
                            drawnMarkers.Add(marker.EndPoint);

                            using (new SKAutoCanvasRestore(canvas))
                            {
                                DrawClimbMarker(canvas, SkiaPaints.MarkerSegmentEndPaint, marker.EndAngle,
                                    marker.EndDrawPoint);
                            }
                        }
                    }
                }
            }

            canvas.Restore();

            // Route markers
            if (Sequence != null && Sequence.Any() && RoutePath.Points.Any())
            {
                var startSegment = Sequence.First();
                var endSegment = Sequence.Last();

                var routeStartPoint = startSegment.Direction == SegmentDirection.AtoB
                    ? SegmentPaths[startSegment.SegmentId].Points[0]
                    : SegmentPaths[startSegment.SegmentId].Points[^1];

                var routeEndPoint = endSegment.Direction == SegmentDirection.AtoB
                    ? SegmentPaths[endSegment.SegmentId].Points[^1]
                    : SegmentPaths[endSegment.SegmentId].Points[0];

                var mappedRouteStartPoint = LogicalMatrix.MapPoint(routeStartPoint);
                var mappedRouteEndPoint = LogicalMatrix.MapPoint(routeEndPoint);

                // Route end marker
                DrawCircleMarker(canvas, mappedRouteEndPoint, SkiaPaints.EndMarkerFillPaint);

                // Route start marker, needs to be after the end marker to
                // ensure the start is always visible if the route starts and
                // ends at the same location.
                DrawCircleMarker(canvas, mappedRouteStartPoint, SkiaPaints.StartMarkerFillPaint);
            }

            if (RiderPosition != null)
            {
                var mappedRiderPosition = LogicalMatrix.MapPoint(RiderPosition.Value);
                DrawCircleMarker(canvas, mappedRiderPosition, SkiaPaints.RiderPositionFillPaint);
            }

            canvas.Flush();
        }

        public World? World { get; set; }

        private bool IsSpawnPointSegment(string segmentId)
        {
            if (World == null)
            {
                return false;
            }

            return World.SpawnPoints.Any(s => s.SegmentId == segmentId);
        }

        public List<RouteSegmentSequence>? Sequence { get; set; }
        public bool OnlyShowRoute { get; set; }

        private void RenderZwiftMap(SKCanvas canvas, World world)
        {
            if (_worldImage == null || _worldImageId != world.ZwiftId)
            {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

                if (assets != null)
                {
                    var stream = assets.Open(new Uri($"avares://RoadCaptain.App.Shared/Assets/map-{world.Id}.png"));
                    _worldImage = SKImage.FromEncodedData(stream);
                    _worldImageId = world.ZwiftId;
                }
            }

            if (_worldImage != null)
            {
                canvas.Save();
                canvas.Scale(ZwiftMapScaleX, ZwiftMapScaleY, 0, 0);
                canvas.Translate(ZwiftMapTranslateX, ZwiftMapTranslateY);
                canvas.DrawImage(_worldImage, 0, 0);
                canvas.Restore();
            }
        }

        private void ScaleAndTranslate(SKCanvas canvas)
        {
            // Start by resetting the logical matrix because otherwise
            // we'd keep adding to it over and over again which is not
            // very sensible...
            LogicalMatrix = canvas.TotalMatrix;

            if (Pan.X != 0 || Pan.Y != 0)
            {
                var translationMatrix = SKMatrix.CreateTranslation(-(float)Pan.X, -(float)Pan.Y);
                LogicalMatrix = translationMatrix;
                canvas.SetMatrix(canvas.TotalMatrix.PostConcat(translationMatrix));
            }

            if (Math.Abs(ZoomLevel - 1) > 0.01)
            {
                var scaleMatrix = SKMatrix.CreateScale(ZoomLevel, ZoomLevel, (float)ZoomCenter.X, (float)ZoomCenter.Y);

                LogicalMatrix = LogicalMatrix != SKMatrix.Empty
                    ? LogicalMatrix.PostConcat(scaleMatrix)
                    : scaleMatrix;

                canvas.SetMatrix(canvas.TotalMatrix.PostConcat(scaleMatrix));
            }
        }

        private static void DrawCircleMarker(SKCanvas canvas, SKPoint point, SKPaint fill)
        {
            canvas.DrawCircle(point, CircleMarkerRadius, SkiaPaints.CircleMarkerPaint);
            canvas.DrawCircle(point, CircleMarkerRadius - SkiaPaints.CircleMarkerPaint.StrokeWidth, fill);
        }

        private static void DrawClimbMarker(SKCanvas canvas, SKPaint paint, float angle, SKPoint point)
        {
            canvas.RotateDegrees(angle, point.X, point.Y);

            canvas.DrawRect(
                point.X - KomMarkerCenterX,
                point.Y - KomMarkerCenterY,
                KomMarkerWidth,
                KomMarkerHeight,
                paint);
        }

        private void InitializeBitmap()
        {
            _bitmap = new SKBitmap((int)Bounds.Width, (int)Bounds.Height, SKColorType.RgbaF16, SKAlphaType.Opaque);

            using var canvas = new SKCanvas(_bitmap);
            canvas.Clear(CanvasBackgroundColor);
        }
    }
}
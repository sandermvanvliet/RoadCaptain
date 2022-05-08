using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using RoadCaptain.App.RouteBuilder.ViewModels;
using SkiaSharp;

namespace RoadCaptain.App.RouteBuilder.Controls
{
    internal class MapRenderOperation : ICustomDrawOperation
    {
        private static readonly SKColor CanvasBackgroundColor = SKColor.Parse("#FFFFFF");
        public SKMatrix MainMatrix { get; private set; }
        public SKMatrix LogicalMatrix { get; private set; }
        public SKMatrix FinalMatrix { get; private set; }

        private const int KomMarkerHeight = 32;
        private const int KomMarkerWidth = 6;
        private const int KomMarkerCenterX = 3;
        private protected const int KomMarkerCenterY = 16;
        private const int CircleMarkerRadius = 10;

        public void Dispose()
        {
        }

        public Rect Bounds { get; set; }

        public bool HitTest(Point p) => false;
        public bool Equals(ICustomDrawOperation? other) => false;
        public string? HighlightedSegmentId { get; set; }
        public string? SelectedSegmentId { get; set; }
        public Point Pan { get; set; } = new(0, 0);
        public float ZoomLevel { get; set; } = 1;
        public Point ZoomCenter { get; set; } = new(0, 0);
        public MainWindowViewModel? ViewModel { get; set; }
        public bool ShowSprints { get; set; }
        public bool ShowClimbs { get; set; }
        public Point ClickedPosition { get; set; }

        public void Render(IDrawingContextImpl context)
        {
            var canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;

            if (canvas == null)
            {
                return;
            }

            MainMatrix = canvas.TotalMatrix;

            DrawCircleMarker(canvas, new SKPoint(100, 100), new SKPaint() {Color = SKColor.Parse("#ff0000"), Style = SKPaintStyle.Fill});

            canvas.Save();

            canvas.Clear(CanvasBackgroundColor);

            RenderCanvas(canvas);

            canvas.Restore();
        }

        private void RenderCanvas(SKCanvas canvas)
        {
            var translationMatrix = SKMatrix.Empty;
            var scaleMatrix = SKMatrix.Empty;

            if (Pan.X != 0 || Pan.Y != 0)
            {
                translationMatrix = SKMatrix.CreateTranslation(-(float)Pan.X, -(float)Pan.Y);
            }

            if (Math.Abs(ZoomLevel - 1) > 0.01)
            {
                scaleMatrix = SKMatrix.CreateScale(ZoomLevel, ZoomLevel, (float)ZoomCenter.X, (float)ZoomCenter.Y);
            }

            LogicalMatrix = translationMatrix;

            if (translationMatrix != SKMatrix.Empty)
            {
                var postConcat = canvas.TotalMatrix.PostConcat(translationMatrix);
                canvas.SetMatrix(postConcat);
            }

            if (scaleMatrix != SKMatrix.Empty)
            {
                var postConcat = canvas.TotalMatrix.PostConcat(scaleMatrix);
                canvas.SetMatrix(postConcat);
            }

            FinalMatrix = canvas.TotalMatrix;

            canvas.DrawPath(ViewModel.RoutePath, SkiaPaints.RoutePathPaint);

            // Lowest layer are the segments
            foreach (var (segmentId, skiaPath) in ViewModel.SegmentPaths)
            {
                SKPaint segmentPaint;

                // Use a different color for the selected segment
                if (segmentId == SelectedSegmentId)
                {
                    segmentPaint = SkiaPaints.SelectedSegmentPathPaint;
                }
                else if (segmentId == HighlightedSegmentId)
                {
                    segmentPaint = SkiaPaints.SegmentHighlightPaint;
                }
                else if (ViewModel.Route.Last == null && ViewModel.Route.IsSpawnPointSegment(segmentId))
                {
                    segmentPaint = SkiaPaints.SpawnPointSegmentPathPaint;
                }
                else
                {
                    segmentPaint = SkiaPaints.SegmentPathPaint;
                }

                canvas.DrawPath(skiaPath, segmentPaint);
            }

            if (ShowClimbs || ShowSprints)
            {
                var drawnMarkers = new List<TrackPoint>();

                foreach (var (_, marker) in ViewModel.Markers)
                {
                    if (marker.Type == SegmentType.Climb && ShowClimbs)
                    {
                        canvas.DrawPath(marker.Path, SkiaPaints.ClimbSegmentPaint);

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
                        canvas.DrawPath(marker.Path, SkiaPaints.SprintSegmentPaint);

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

            // Route markers
            if (ViewModel.RoutePath.Points.Any())
            {
                // Route end marker
                DrawCircleMarker(canvas, ViewModel.RoutePath.Points.Last(), SkiaPaints.EndMarkerFillPaint);

                // Route start marker, needs to be after the end marker to
                // ensure the start is always visible if the route starts and
                // ends at the same location.
                DrawCircleMarker(canvas, ViewModel.RoutePath.Points.First(), SkiaPaints.StartMarkerFillPaint);
            }

            if (ViewModel.RiderPosition != null)
            {
                DrawCircleMarker(canvas, ViewModel.RiderPosition.Value, SkiaPaints.RiderPositionFillPaint);
            }

            DrawCircleMarker(canvas, new SKPoint(100, 100), new SKPaint() {Color = SKColor.Parse("#0000ff"), Style = SKPaintStyle.Fill});
            DrawCircleMarker(canvas, new SKPoint(100, 10), new SKPaint() {Color = SKColor.Parse("#0000ff"), Style = SKPaintStyle.Fill});
            DrawCircleMarker(canvas, new SKPoint(10, 100), new SKPaint() {Color = SKColor.Parse("#0000ff"), Style = SKPaintStyle.Fill});
            DrawCircleMarker(canvas, new SKPoint(200, 200), new SKPaint() {Color = SKColor.Parse("#0000ff"), Style = SKPaintStyle.Fill});

            if (ClickedPosition != new Point(0, 0))
            {
                DrawCircleMarker(canvas, new SKPoint((float)ClickedPosition.X, (float)ClickedPosition.Y), SkiaPaints.RiderPositionFillPaint);
            }

            canvas.Flush();
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
    }
}
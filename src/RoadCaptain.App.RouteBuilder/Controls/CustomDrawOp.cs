using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using RoadCaptain.App.RouteBuilder.ViewModels;
using SkiaSharp;

namespace RoadCaptain.App.RouteBuilder.Controls
{
    internal class CustomDrawOp : ICustomDrawOperation
    {
        private const int KomMarkerHeight = 32;
        private const int KomMarkerWidth = 6;

        private readonly FormattedText _noSkia;

        public CustomDrawOp(Rect bounds, FormattedText noSkia, MainWindowViewModel viewModel)
        {
            _noSkia = noSkia;
            Bounds = bounds;
            ViewModel = viewModel;
        }

        public void Dispose()
        {
            // No-op
        }

        public Rect Bounds { get; }
        public MainWindowViewModel ViewModel { get; }
        
        public bool HitTest(Point p) => false;
        
        public bool Equals(ICustomDrawOperation other) => false;

        public SKMatrix CurrentMatrix { get; private set; }

        public string? HighlightedSegmentId { get; set; }

        public void Render(IDrawingContextImpl context)
        {
            var canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;
            if (canvas == null)
            {
                context.DrawText(Brushes.Black, new Point(), _noSkia.PlatformImpl);
                return;
            }

            canvas.Save();
           
            RenderCanvas(canvas);

            canvas.Restore();
        }
        
        
        private void RenderCanvas(SKCanvas canvas)
        {
            canvas.Translate(-(float)ViewModel.Pan.X, -(float)ViewModel.Pan.Y);
            canvas.Scale(ViewModel.Zoom, ViewModel.Zoom, (float)ViewModel.ZoomCenter.X, (float)ViewModel.ZoomCenter.Y);

            // Store the inverse of the scale/translate matrix
            // so that we can convert a click on the canvas to
            // the correct coordinates of a segment.
            CurrentMatrix = canvas.TotalMatrix.Invert();

            canvas.Clear(SKColor.Parse("#FFFFFF"));

            canvas.DrawPath(ViewModel.RoutePath, SkiaPaints.RoutePathPaint);

            // Lowest layer are the segments
            foreach (var (segmentId, skiaPath) in ViewModel.SegmentPaths)
            {
                SKPaint segmentPaint;

                // Use a different color for the selected segment
                if (segmentId == ViewModel.SelectedSegment?.Id)
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

            if (ViewModel.ShowClimbs || ViewModel.ShowSprints)
            {
                var drawnMarkers = new List<TrackPoint>();

                foreach (var (_, marker) in ViewModel.Markers)
                {
                    if (marker.Type == SegmentType.Climb && ViewModel.ShowClimbs)
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
                    else if (marker.Type == SegmentType.Sprint && ViewModel.ShowSprints)
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
                var endPoint = ViewModel.RoutePath.Points.Last();

                canvas.DrawCircle(endPoint, 15, SkiaPaints.StartMarkerPaint);
                canvas.DrawCircle(endPoint, 15 - SkiaPaints.StartMarkerPaint.StrokeWidth, SkiaPaints.EndMarkerFillPaint);

                // Route start marker, needs to be after the end marker to
                // ensure the start is always visible if the route starts and
                // ends at the same location.
                var startPoint = ViewModel.RoutePath.Points.First();

                canvas.DrawCircle(startPoint, 15, SkiaPaints.StartMarkerPaint);
                canvas.DrawCircle(startPoint, 15 - SkiaPaints.StartMarkerPaint.StrokeWidth, SkiaPaints.StartMarkerFillPaint);
            }

            if (ViewModel.RiderPosition != null)
            {
                var scaledAndTranslated = ViewModel.RiderPosition.Value;
                const int radius = 15;
                canvas
                    .DrawCircle(scaledAndTranslated.X, scaledAndTranslated.Y, radius, SkiaPaints.RiderPositionPaint);
                canvas
                    .DrawCircle(scaledAndTranslated.X, scaledAndTranslated.Y, radius - SkiaPaints.RiderPositionPaint.StrokeWidth, SkiaPaints.RiderPositionFillPaint);
            }

            canvas.Flush();
        }

        private void DrawClimbMarker(SKCanvas canvas, SKPaint paint, float angle, SKPoint point)
        {
            canvas.RotateDegrees(angle, point.X, point.Y);

            canvas.DrawRect(
                point.X - KomMarkerWidth / 2,
                point.Y - KomMarkerHeight / 2,
                KomMarkerWidth,
                KomMarkerHeight,
                paint);
        }
    }
}
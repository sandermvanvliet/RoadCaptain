using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using RoadCaptain.App.RouteBuilder.ViewModels;
using SkiaSharp;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public partial class MainWindow : Window
    {
        private const int KomMarkerHeight = 32;
        private const int KomMarkerWidth = 6;

        private SKMatrix? _currentMatrix;
        private string? _highlightedSegmentId;

        // ReSharper disable once UnusedMember.Global because this constructor only exists for the Avalonia designer
        public MainWindow()
        {
        }

        public MainWindow(MainWindowViewModel viewModel) 
        {
            ViewModel = viewModel;
            ViewModel.PropertyChanged += WindowViewModelPropertyChanged;
            DataContext = viewModel;

            InitializeComponent();
        }

        private MainWindowViewModel ViewModel { get; }

        private void WindowViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // TODO: Use reactive approach with WhenXxx() for this
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SelectedSegment):
                case nameof(ViewModel.SegmentPaths):
                    // Reset any manually selected item in the list
                    _highlightedSegmentId = null;
                    TriggerRepaint();
                    break;
                case nameof(ViewModel.Route):
                    // Ensure the last added segment is visible
                    if (RouteListView.ItemCount > 0)
                    {
                        RouteListView.ScrollIntoView(RouteListView.ItemCount - 1);
                    }

                    // When a world is selected the path segments
                    // need to be generated which needs the canvas
                    // size. Therefore we need to call that from
                    // this handler
                    if (ViewModel.Route.World != null && !ViewModel.SegmentPaths.Any())
                    {
                        ViewModel.CreatePathsForSegments((float)SkElement.Width, (float)SkElement.Height);
                    }

                    // Redraw when the route changes so that the
                    // route path is painted correctly
                    TriggerRepaint();
                    break;
                case nameof(ViewModel.RiderPosition):
                case nameof(ViewModel.ShowClimbs):
                case nameof(ViewModel.ShowSprints):
                case nameof(ViewModel.Zoom):
                case nameof(ViewModel.Pan):
                    TriggerRepaint();
                    break;
            }
        }


        private void TriggerRepaint()
        {
            if (SkElement.CheckAccess())
            {
                SkElement.InvalidateVisual();
            }
            else
            {
                Dispatcher.UIThread.InvokeAsync(TriggerRepaint);
            }
        }

        // TODO: Implement Skia canvas painting
        /*
        private void SKElement_OnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            // Purely for readability
            var canvas = args.Surface.Canvas;

            canvas.Translate(-(float)ViewModel.Pan.X, -(float)ViewModel.Pan.Y);
            canvas.Scale(ViewModel.Zoom, ViewModel.Zoom, (float)ViewModel.ZoomCenter.X, (float)ViewModel.ZoomCenter.Y);

            // Store the inverse of the scale/translate matrix
            // so that we can convert a click on the canvas to
            // the correct coordinates of a segment.
            _currentMatrix = canvas.TotalMatrix.Invert();

            canvas.Clear();

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
                else if (segmentId == _highlightedSegmentId)
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
        */

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

        private void MainWindow_Initialized(object? sender, EventArgs eventArgs)
        {
            // TODO: Fix Skia canvas rendering
            //ViewModel.CreatePathsForSegments(SkElement.CanvasSize.Width, SkElement.CanvasSize.Height);
        }

        private void SkElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (sender is not Canvas skiaElement)
            {
                return;
            }

            if (ViewModel.IsPanning)
            {
                ViewModel.EndPan();
                return;
            }

            var position = e.GetPosition((IInputElement)sender);

            var canvasCoordinate = ConvertMousePositionToCanvasCoordinate(skiaElement, position);

            ViewModel.SelectSegmentCommand.Execute(canvasCoordinate);
        }

        private void RouteListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox { SelectedItem: SegmentSequenceViewModel viewModel })
            {
                _highlightedSegmentId = viewModel.SegmentId;
                TriggerRepaint();
            }
            else
            {
                _highlightedSegmentId = null;
            }
        }

        private void RouteListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender is ListBox { SelectedItem: SegmentSequenceViewModel viewModel } && e.Key == Key.Delete)
            {
                if (viewModel == ViewModel.Route.Last)
                {
                    ViewModel.RemoveLastSegmentCommand.Execute(null);
                    if (RouteListView.ItemCount > 0)
                    {
                        RouteListView.SelectedItem = RouteListView.Items.Cast<object>().Last();
                    }
                }
            }
        }

        private void MainWindow_OnActivated(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => ViewModel.CheckForNewVersion());
        }

        private void SkElement_OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (sender is not Canvas skiaElement)
            {
            }

            // TODO: Fix this
            //var position = e.GetPosition((IInputElement)sender);

            //if (e.LeftButton == MouseButtonState.Pressed)
            //{
            //    if (!ViewModel.IsPanning)
            //    {
            //        ViewModel.StartPan(position);
            //    }
            //    else
            //    {
            //        ViewModel.PanMove(position);
            //    }

            //    return;
            //}

            //if (ViewModel.IsPanning)
            //{
            //    ViewModel.EndPan();
            //    return;
            //}

            //// Hit test to see whether we're over a KOM/Sprint segment

            //// If sprints and climbs are not shown then exit
            //if (!ViewModel.ShowSprints && !ViewModel.ShowClimbs)
            //{
            //    return;
            //}

            //var scaledPoint = ConvertMousePositionToCanvasCoordinate(skiaElement, position);

            //var matches = ViewModel
            //    .Markers
            //    .Values
            //    .Where(kv => kv.Bounds.Contains((float)scaledPoint.X, (float)scaledPoint.Y))
            //    .ToList();

            //if (matches.Count == 1)
            //{
            //    var marker = matches.Single();

            //    ViewModel.Model.StatusBarInfo("{0} {1}", marker.Type.ToString(), marker.Name);
            //}
            //else
            //{
            //    ViewModel.Model.ClearStatusBar();
            //}
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ZoomIn(ConvertMousePositionToCanvasCoordinate(SkElement,
                new Point(SkElement.Width / 2, SkElement.Height / 2)));

            TriggerRepaint();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ZoomOut(ConvertMousePositionToCanvasCoordinate(SkElement,
                new Point(SkElement.Width / 2, SkElement.Height / 2)));

            TriggerRepaint();
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ResetZoomAndPan();

            TriggerRepaint();
        }

        private Point ConvertMousePositionToCanvasCoordinate(Canvas skiaElement, Point position)
        {
            // This is the canvas to WPF element scaling, not the canvas scaling itself
            var scalingFactor = skiaElement.Width / skiaElement.Width;

            var matrixConverted = _currentMatrix.Value.MapPoint(
                (float)(position.X * scalingFactor),
                (float)(position.Y * scalingFactor));

            return new Point(matrixConverted.X, matrixConverted.Y);
        }

        //private void SkElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    var skiaElement = sender as Canvas;

        //    var position = e.GetPosition((IInputElement)sender);

        //    var canvasCoordinate = ConvertMousePositionToCanvasCoordinate(skiaElement, position);

        //    if (e.Delta > 0)
        //    {
        //        ViewModel.ZoomIn(canvasCoordinate);
        //    }
        //    else if (e.Delta < 0)
        //    {
        //        ViewModel.ZoomOut(canvasCoordinate);
        //    }
        //}

        private void AvaloniaObject_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == nameof(ClientSize))
            {
                // TODO: Fix Skia canvas rendering
                //ViewModel.CreatePathsForSegments(SkElement.CanvasSize.Width, SkElement.CanvasSize.Height);
            }
        }
    }
}
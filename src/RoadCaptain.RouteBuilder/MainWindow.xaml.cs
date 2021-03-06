// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RoadCaptain.RouteBuilder.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using DispatcherPriority = System.Windows.Threading.DispatcherPriority;
using Point = System.Windows.Point;

namespace RoadCaptain.RouteBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : Window
    {
        private const int KomMarkerHeight = 32;
        private const int KomMarkerWidth = 6;

        private readonly MainWindowViewModel _windowViewModel;
        private string _highlightedSegmentId;
        private SKMatrix _currentMatrix;

        public MainWindow(MainWindowViewModel mainWindowViewModel)
        {
            _windowViewModel = mainWindowViewModel;
            DataContext = mainWindowViewModel;

            _windowViewModel.PropertyChanged += WindowViewModelPropertyChanged;

            InitializeComponent();
        }

        private void WindowViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_windowViewModel.SelectedSegment):
                case nameof(_windowViewModel.SegmentPaths):
                    // Reset any manually selected item in the list
                    _highlightedSegmentId = null;
                    TriggerRepaint();
                    break;
                case nameof(_windowViewModel.Route):
                    // Ensure the last added segment is visible
                    if (RouteListView.Items.Count > 0)
                    {
                        RouteListView.ScrollIntoView(RouteListView.Items[^1]);
                    }

                    // When a world is selected the path segments
                    // need to be generated which needs the canvas
                    // size. Therefore we need to call that from
                    // this handler
                    if (_windowViewModel.Route.World != null && !_windowViewModel.SegmentPaths.Any())
                    {
                        _windowViewModel.CreatePathsForSegments(SkElement.CanvasSize.Width, SkElement.CanvasSize.Height);
                    }

                    // Redraw when the route changes so that the
                    // route path is painted correctly
                    TriggerRepaint();
                    break;
                case nameof(_windowViewModel.RiderPosition):
                case nameof(_windowViewModel.ShowClimbs):
                case nameof(_windowViewModel.ShowSprints):
                case nameof(_windowViewModel.Zoom):
                case nameof(_windowViewModel.Pan):
                    TriggerRepaint();
                    break;
            }
        }

        private void TriggerRepaint()
        {
            if (SkElement.Dispatcher.CheckAccess())
            {
                SkElement.InvalidateVisual();
            }
            else
            {
                SkElement.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action)TriggerRepaint);
            }
        }

        private void SKElement_OnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            // Purely for readability
            var canvas = args.Surface.Canvas;

            canvas.Translate(-(float)_windowViewModel.Pan.X, -(float)_windowViewModel.Pan.Y);
            canvas.Scale(_windowViewModel.Zoom, _windowViewModel.Zoom, (float)_windowViewModel.ZoomCenter.X, (float)_windowViewModel.ZoomCenter.Y);

            // Store the inverse of the scale/translate matrix
            // so that we can convert a click on the canvas to
            // the correct coordinates of a segment.
            _currentMatrix = canvas.TotalMatrix.Invert();

            canvas.Clear();

            canvas.DrawPath(_windowViewModel.RoutePath, SkiaPaints.RoutePathPaint);

            // Lowest layer are the segments
            foreach (var (segmentId, skiaPath) in _windowViewModel.SegmentPaths)
            {
                SKPaint segmentPaint;

                // Use a different color for the selected segment
                if (segmentId == _windowViewModel.SelectedSegment?.Id)
                {
                    segmentPaint = SkiaPaints.SelectedSegmentPathPaint;
                }
                else if (segmentId == _highlightedSegmentId)
                {
                    segmentPaint = SkiaPaints.SegmentHighlightPaint;
                }
                else if (_windowViewModel.Route.Last == null && _windowViewModel.Route.IsSpawnPointSegment(segmentId))
                {
                    segmentPaint = SkiaPaints.SpawnPointSegmentPathPaint;
                }
                else
                {
                    segmentPaint = SkiaPaints.SegmentPathPaint;
                }

                canvas.DrawPath(skiaPath, segmentPaint);
            }

            if (_windowViewModel.ShowClimbs || _windowViewModel.ShowSprints)
            {
                var drawnMarkers = new List<TrackPoint>();

                foreach (var (_, marker) in _windowViewModel.Markers)
                {
                    if (marker.Type == SegmentType.Climb && _windowViewModel.ShowClimbs)
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
                    else if (marker.Type == SegmentType.Sprint && _windowViewModel.ShowSprints)
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
            if (_windowViewModel.RoutePath.Points.Any())
            {
                // Route end marker
                var endPoint = _windowViewModel.RoutePath.Points.Last();

                canvas.DrawCircle(endPoint, 15, SkiaPaints.StartMarkerPaint);
                canvas.DrawCircle(endPoint, 15 - SkiaPaints.StartMarkerPaint.StrokeWidth, SkiaPaints.EndMarkerFillPaint);

                // Route start marker, needs to be after the end marker to
                // ensure the start is always visible if the route starts and
                // ends at the same location.
                var startPoint = _windowViewModel.RoutePath.Points.First();

                canvas.DrawCircle(startPoint, 15, SkiaPaints.StartMarkerPaint);
                canvas.DrawCircle(startPoint, 15 - SkiaPaints.StartMarkerPaint.StrokeWidth, SkiaPaints.StartMarkerFillPaint);
            }

            if (_windowViewModel.RiderPosition != null)
            {
                var scaledAndTranslated = _windowViewModel.RiderPosition.Value;
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
                point.X - (KomMarkerWidth / 2),
                point.Y - (KomMarkerHeight / 2),
                KomMarkerWidth,
                KomMarkerHeight,
                paint);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _windowViewModel.CreatePathsForSegments(SkElement.CanvasSize.Width, SkElement.CanvasSize.Height);
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowViewModel.CreatePathsForSegments(SkElement.CanvasSize.Width, SkElement.CanvasSize.Height);
        }

        private void SkElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not SKElement skiaElement)
            {
                return;
            }

            if (_windowViewModel.IsPanning)
            {
                _windowViewModel.EndPan();
                return;
            }

            var position = e.GetPosition((IInputElement)sender);

            var canvasCoordinate = ConvertMousePositionToCanvasCoordinate(skiaElement, position);

            _windowViewModel.SelectSegmentCommand.Execute(canvasCoordinate);
        }

        private void RouteListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView { SelectedItem: SegmentSequenceViewModel viewModel })
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
            if (sender is ListView { SelectedItem: SegmentSequenceViewModel viewModel } && e.Key == Key.Delete)
            {
                if (viewModel == _windowViewModel.Route.Last)
                {
                    _windowViewModel.RemoveLastSegmentCommand.Execute(null);
                    if (RouteListView.HasItems)
                    {
                        RouteListView.SelectedItem = RouteListView.Items[^1];
                    }
                }
            }
        }

        private void MainWindow_OnActivated(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => _windowViewModel.CheckForNewVersion());
        }

        private void SkElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not SKElement skiaElement)
            {
                return;
            }

            var position = e.GetPosition((IInputElement)sender);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!_windowViewModel.IsPanning)
                {
                    _windowViewModel.StartPan(position);
                }
                else
                {
                    _windowViewModel.PanMove(position);
                }

                return;
            }

            if (_windowViewModel.IsPanning)
            {
                _windowViewModel.EndPan();
                return;
            }

            // Hit test to see whether we're over a KOM/Sprint segment

            // If sprints and climbs are not shown then exit
            if (!_windowViewModel.ShowSprints && !_windowViewModel.ShowClimbs)
            {
                return;
            }

            var scaledPoint = ConvertMousePositionToCanvasCoordinate(skiaElement, position);

            var matches = _windowViewModel
                .Markers
                .Values
                .Where(kv => kv.Bounds.Contains((float)scaledPoint.X, (float)scaledPoint.Y))
                .ToList();

            if (matches.Count == 1)
            {
                var marker = matches.Single();

                _windowViewModel.Model.StatusBarInfo("{0} {1}", marker.Type.ToString(), marker.Name);
            }
            else
            {
                _windowViewModel.Model.ClearStatusBar();
            }
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            _windowViewModel.ZoomIn(ConvertMousePositionToCanvasCoordinate(SkElement, new Point(SkElement.ActualWidth / 2, SkElement.ActualHeight / 2)));

            TriggerRepaint();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            _windowViewModel.ZoomOut(ConvertMousePositionToCanvasCoordinate(SkElement, new Point(SkElement.ActualWidth / 2, SkElement.ActualHeight / 2)));

            TriggerRepaint();
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            _windowViewModel.ResetZoomAndPan();

            TriggerRepaint();
        }

        private Point ConvertMousePositionToCanvasCoordinate(SKElement skiaElement, Point position)
        {
            // This is the canvas to WPF element scaling, not the canvas scaling itself
            var scalingFactor = skiaElement.CanvasSize.Width / skiaElement.ActualWidth;

            var matrixConverted = _currentMatrix.MapPoint(
                    (float)(position.X * scalingFactor),
                    (float)(position.Y * scalingFactor));

            return new Point(matrixConverted.X, matrixConverted.Y);
        }

        private void SkElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var skiaElement = sender as SKElement;

            var position = e.GetPosition((IInputElement)sender);

            var canvasCoordinate = ConvertMousePositionToCanvasCoordinate(skiaElement, position);

            if (e.Delta > 0)
            {
                _windowViewModel.ZoomIn(canvasCoordinate);
            }
            else if (e.Delta < 0)
            {
                _windowViewModel.ZoomOut(canvasCoordinate);
            }
        }
    }
}


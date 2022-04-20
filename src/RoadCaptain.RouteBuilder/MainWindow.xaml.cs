// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
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
        private bool _showClimbs = false;

        private readonly SKPaint _segmentPathPaint = new()
            { Color = SKColor.Parse("#000000"), Style = SKPaintStyle.Stroke, StrokeWidth = 4 };

        private readonly SKPaint _selectedSegmentPathPaint = new()
            { Color = SKColor.Parse("#ffcc00"), Style = SKPaintStyle.Stroke, StrokeWidth = 6 };

        private readonly SKPaint _segmentHighlightPaint = new()
            { Color = SKColor.Parse("#4CFF00"), Style = SKPaintStyle.Stroke, StrokeWidth = 6 };

        private readonly SKPaint _spawnPointSegmentPathPaint = new()
            { Color = SKColor.Parse("#44dd44"), Style = SKPaintStyle.Stroke, StrokeWidth = 4 };

        private readonly SKPaint _routePathPaint = new()
            { Color = SKColor.Parse("#0000ff"), Style = SKPaintStyle.Stroke, StrokeWidth = 8 };

        private readonly SKPaint _riderPositionPaint = new()
            { Color = SKColor.Parse("#ffffff"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 4 };

        private readonly SKPaint _riderPositionFillPaint = new()
            { Color = SKColor.Parse("#FF6141"), Style = SKPaintStyle.Fill };

        private readonly SKPaint _startMarkerPaint = new()
            { Color = SKColor.Parse("#ffffff"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 4 };

        private readonly SKPaint _startMarkerFillPaint = new()
            { Color = SKColor.Parse("#14c817"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 0 };

        private readonly SKPaint _endMarkerFillPaint = new()
            { Color = SKColor.Parse("#ff0000"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 0 };

        private readonly SKPaint _markerSegmentStartPaint = new()
            { Color = SKColor.Parse("#ff0000"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 0 };
        
        private readonly SKPaint _markerSegmentEndPaint = new()
            { Color = SKColor.Parse("#14c817"), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 0 };

        private readonly MainWindowViewModel _windowViewModel;
        private string _highlightedSegmentId;

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

            canvas.Clear();

            canvas.DrawPath(_windowViewModel.RoutePath, _routePathPaint);
            
            // Lowest layer are the segments
            foreach (var (segmentId, skiaPath) in _windowViewModel.SegmentPaths)
            {
                SKPaint segmentPaint;

                // Use a different color for the selected segment
                if (segmentId == _windowViewModel.SelectedSegment?.Id)
                {
                    segmentPaint = _selectedSegmentPathPaint;
                }
                else if (segmentId == _highlightedSegmentId)
                {
                    segmentPaint = _segmentHighlightPaint;
                }
                else if (_windowViewModel.Route.Last == null && _windowViewModel.Route.IsSpawnPointSegment(segmentId))
                {
                    segmentPaint = _spawnPointSegmentPathPaint;
                }
                else
                {
                    segmentPaint = _segmentPathPaint;
                }

                canvas.DrawPath(skiaPath, segmentPaint);
            }

            if (_showClimbs)
            {
                foreach (var (segmentId, marker) in _windowViewModel.Markers)
                {

                    using (new SKAutoCanvasRestore(canvas))
                    {
                        // do any transformations
                        canvas.RotateDegrees(marker.StartAngle, marker.StartPoint.X, marker.StartPoint.Y);
                        // do serious work
                        canvas.DrawRect(marker.StartPoint.X - (KomMarkerWidth / 2),
                            marker.StartPoint.Y - (KomMarkerHeight / 2), KomMarkerWidth, KomMarkerHeight,
                            _markerSegmentStartPaint);
                        // auto restore, even on exceptions or errors
                    }

                    using (new SKAutoCanvasRestore(canvas))
                    {
                        // do any transformations
                        canvas.RotateDegrees(marker.EndAngle, marker.EndPoint.X, marker.EndPoint.Y);
                        // do serious work
                        canvas.DrawRect(marker.EndPoint.X - (KomMarkerWidth / 2),
                            marker.EndPoint.Y - (KomMarkerHeight / 2), KomMarkerWidth, KomMarkerHeight,
                            _markerSegmentEndPaint);
                        // auto restore, even on exceptions or errors
                    }
                }
            }

            // Route markers
            if (_windowViewModel.RoutePath.Points.Any())
            {
                // Route end marker
                var endPoint = _windowViewModel.RoutePath.Points.Last();
                
                canvas.DrawCircle(endPoint, 15, _startMarkerPaint);
                canvas.DrawCircle(endPoint, 15 - _startMarkerPaint.StrokeWidth, _endMarkerFillPaint);
            
                // Route start marker, needs to be after the end marker to
                // ensure the start is always visible if the route starts and
                // ends at the same location.
                var startPoint = _windowViewModel.RoutePath.Points.First();
                
                canvas.DrawCircle(startPoint, 15, _startMarkerPaint);
                canvas.DrawCircle(startPoint, 15 - _startMarkerPaint.StrokeWidth, _startMarkerFillPaint);
            }

            if (_windowViewModel.RiderPosition != null)
            {
                var scaledAndTranslated = _windowViewModel.RiderPosition.Value;
                const int radius = 15;
                canvas
                    .DrawCircle(scaledAndTranslated.X, scaledAndTranslated.Y, radius, _riderPositionPaint);
                canvas
                    .DrawCircle(scaledAndTranslated.X, scaledAndTranslated.Y, radius - _riderPositionPaint.StrokeWidth, _riderPositionFillPaint);
            }

            canvas.Flush();
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

            var position = e.GetPosition((IInputElement)sender);

            var scalingFactor = skiaElement.CanvasSize.Width / skiaElement.ActualWidth;
            var scaledPoint = new Point(position.X * scalingFactor, position.Y * scalingFactor);
            
            _windowViewModel.SelectSegmentCommand.Execute(scaledPoint);
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
    }
}


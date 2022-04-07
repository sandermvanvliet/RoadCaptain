// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
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
            _windowViewModel.CreatePathsForSegments(SkElement.CanvasSize.Width);
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowViewModel.CreatePathsForSegments(SkElement.CanvasSize.Width);
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
    }
}


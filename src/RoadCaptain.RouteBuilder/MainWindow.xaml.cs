using System.Linq;
using System.Windows;
using System.Windows.Input;
using RoadCaptain.RouteBuilder.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
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

        private readonly MainViewModel _viewModel = new();

        public MainWindow()
        {
            DataContext = _viewModel;

            _viewModel.PropertyChanged += _viewModel_PropertyChanged;

            InitializeComponent();
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_viewModel.SelectedSegment):
                case nameof(_viewModel.SegmentPaths):
                    SkElement.InvalidateVisual();
                    break;
            }
        }

        private void SKElement_OnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            args.Surface.Canvas.Clear();
            
            // Lowest layer are the segments
            foreach (var skPath in _viewModel.SegmentPaths)
            {
                SKPaint segmentPaint;

                // Use a different color for the selected segment
                if (_viewModel.SelectedSegment != null && skPath.Key == _viewModel.SelectedSegment.Id)
                {
                    segmentPaint = _selectedSegmentPathPaint;
                }
                else
                {
                    segmentPaint = _segmentPathPaint;
                }

                args.Surface.Canvas.DrawPath(skPath.Value, segmentPaint);
            }
            
            args.Surface.Canvas.Flush();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.CreatePathsForSegments(SkElement.CanvasSize.Width);
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_viewModel.Segments?.Any() ?? false)
            {
                _viewModel.CreatePathsForSegments(SkElement.CanvasSize.Width);
            }
        }

        private void SkElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(sender as IInputElement);
            
            // 1. Work out scaling to canvas size
            var scalingFactor = SkElement.CanvasSize.Width / SkElement.ActualWidth;
            var scaledPoint = new Point(position.X * scalingFactor, position.Y * scalingFactor);
            
            // 2. Find SKPath that contains this coordinate (or close enough)
            var pathsInBounds = _viewModel.SegmentPathBounds
                .Where(p => p.Value.Contains((float)scaledPoint.X, (float)scaledPoint.Y))
                .ToList();

            // 3. Highlight it
            if (pathsInBounds.Count == 1)
            {
                // TODO: Do something smart with tightest bounds if that ever becomes necessary
                _viewModel.SelectSegment(pathsInBounds[0].Key);
            }
            else
            {
                _viewModel.ClearSelectedSegment();
            }
        }
    }
}

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

        private readonly MainWindowViewModel _windowViewModel = new();

        public MainWindow()
        {
            DataContext = _windowViewModel;

            _windowViewModel.PropertyChanged += WindowViewModelPropertyChanged;

            InitializeComponent();
        }

        private void WindowViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_windowViewModel.SelectedSegment):
                case nameof(_windowViewModel.SegmentPaths):
                    SkElement.InvalidateVisual();
                    break;
            }
        }

        private void SKElement_OnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            args.Surface.Canvas.Clear();
            
            // Lowest layer are the segments
            foreach (var skPath in _windowViewModel.SegmentPaths)
            {
                SKPaint segmentPaint;

                // Use a different color for the selected segment
                if (_windowViewModel.SelectedSegment != null && skPath.Key == _windowViewModel.SelectedSegment.Id)
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
            _windowViewModel.CreatePathsForSegments(SkElement.CanvasSize.Width);
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_windowViewModel.Segments?.Any() ?? false)
            {
                _windowViewModel.CreatePathsForSegments(SkElement.CanvasSize.Width);
            }
        }

        private void SkElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(sender as IInputElement);
            
            // 1. Work out scaling to canvas size
            var scalingFactor = SkElement.CanvasSize.Width / SkElement.ActualWidth;
            var scaledPoint = new Point(position.X * scalingFactor, position.Y * scalingFactor);
            
            // 2. Find SKPath that contains this coordinate (or close enough)
            var pathsInBounds = _windowViewModel.SegmentPathBounds
                .Where(p => p.Value.Contains((float)scaledPoint.X, (float)scaledPoint.Y))
                .OrderBy(x => x.Value, new SkRectComparer()) // Sort by bounds area, good enough for now
                .ToList();

            // 3. Highlight it
            if (pathsInBounds.Any())
            {
                _windowViewModel.SelectSegmentCommand.Execute(pathsInBounds.First().Key);

                // Ensure the last added segment is visible
                RouteListView.ScrollIntoView(RouteListView.Items[^1]);
            }
            else
            {
                _windowViewModel.ClearSelectedSegment();
            }
        }
    }
}

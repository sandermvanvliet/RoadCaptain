using System.Collections.Generic;
using System.Drawing;
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
        private readonly Dictionary<string, SKPath> _segmentPaths = new();

        private readonly SKPaint _riderPathPaint = new()
            { Color = SKColor.Parse("#0000ff"), Style = SKPaintStyle.Stroke, StrokeWidth = 2 };

        private readonly SKPaint _segmentPathPaint = new()
            { Color = SKColor.Parse("#000000"), Style = SKPaintStyle.Stroke, StrokeWidth = 4 };

        private readonly SKPaint _selectedSegmentPathPaint = new()
            { Color = SKColor.Parse("#ffcc00"), Style = SKPaintStyle.Stroke, StrokeWidth = 6 };

        private readonly SKPath _riderPath = new();
        private Offsets _overallOffsets;
        private readonly Dictionary<string, SKRect> _segmentPathBounds = new();

        private readonly MainViewModel _viewModel = new();

        public MainWindow()
        {
            DataContext = _viewModel;

            _viewModel.PropertyChanged += _viewModel_PropertyChanged;

            InitializeComponent();
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.SelectedSegment))
            {
                SkElement.InvalidateVisual();
            }
        }

        private void SKElement_OnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            args.Surface.Canvas.Clear();
            
            // Lowest layer are the segments
            foreach (var skPath in _segmentPaths)
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

            // Upon that we draw the path as the rider took it
            args.Surface.Canvas.DrawPath(_riderPath, _riderPathPaint);
            
            args.Surface.Canvas.Flush();
        }

        private void CreatePathsForSegments()
        {
            _segmentPaths.Clear();
            _segmentPathBounds.Clear();

            var segmentsWithOffsets = _viewModel.Segments
                .Select(seg => new
                {
                    Segment = seg,
                    GameCoordinates = seg.Points.Select(point =>
                        TrackPoint.LatLongToGame(point.Longitude, -point.Latitude, point.Altitude)).ToList()
                })
                .Select(x => new
                {
                    x.Segment,
                    x.GameCoordinates,
                    Offsets = new Offsets(SkElement.CanvasSize.Width, x.GameCoordinates)
                })
                .ToList();

            _overallOffsets = new Offsets(
                SkElement.CanvasSize.Width,
                segmentsWithOffsets.SelectMany(s => s.GameCoordinates).ToList());

            foreach (var segment in segmentsWithOffsets)
            {
                var skiaPathFromSegment = SkiaPathFromSegment(_overallOffsets, segment.GameCoordinates);
                skiaPathFromSegment.GetTightBounds(out var bounds);

                _segmentPaths.Add(segment.Segment.Id, skiaPathFromSegment);
                _segmentPathBounds.Add(segment.Segment.Id, bounds);
            }
        }

        private static SKPath SkiaPathFromSegment(Offsets offsets, List<TrackPoint> data)
        {
            var path = new SKPath();

            path.AddPoly(
                data
                    .Select(point => ScaleAndTranslate(point, offsets))
                    .Select(point => new SKPoint(point.X, point.Y))
                    .ToArray(),
                false);

            return path;
        }

        private static PointF ScaleAndTranslate(TrackPoint point, Offsets offsets)
        {
            var translatedX = offsets.OffsetX + (float)point.Latitude;
            var translatedY = offsets.OffsetY + (float)point.Longitude;

            var scaledX = translatedX * offsets.ScaleFactor;
            var scaledY = translatedY * offsets.ScaleFactor;

            return new PointF(scaledX, scaledY);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            CreatePathsForSegments();

            SkElement.InvalidateVisual();
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_viewModel.Segments != null)
            {
                CreatePathsForSegments();

                SkElement.InvalidateVisual();
            }
        }

        private void SkElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(sender as IInputElement);
            
            // 1. Work out scaling to canvas size
            var scalingFactor = SkElement.CanvasSize.Width / SkElement.ActualWidth;
            var scaledPoint = new Point(position.X * scalingFactor, position.Y * scalingFactor);
            
            // 2. Find SKPath that contains this coordinate (or close enough)
            var pathsInBounds = _segmentPathBounds
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

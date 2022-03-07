using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RoadCaptain.Adapters;
using RoadCaptain.RouteBuilder.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace RoadCaptain.RouteBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Segment _selectedSegment;
        private readonly Dictionary<string, SKPath> _segmentPaths = new();

        private readonly SKPaint _riderPathPaint = new()
            { Color = SKColor.Parse("#0000ff"), Style = SKPaintStyle.Stroke, StrokeWidth = 2 };

        private readonly SKPaint _segmentPathPaint = new()
            { Color = SKColor.Parse("#000000"), Style = SKPaintStyle.Stroke, StrokeWidth = 4 };

        private readonly SKPaint _selectedSegmentPathPaint = new()
            { Color = SKColor.Parse("#ffcc00"), Style = SKPaintStyle.Stroke, StrokeWidth = 6 };

        private SKPath _riderPath = new();
        private List<Segment> _segments;
        private Offsets _overallOffsets;

        public MainWindow()
        {
            DataContext = new MainViewModel
            {
                Route = new RouteViewModel
                {
                    Sequence = new List<SegmentSequenceViewModel>
                    {
                        new()
                        {
                            Ascent = 10,
                            Descent = 0,
                            Distance = 1.25,
                            Segment = "abcd",
                            SequenceNumber = 1,
                            TurnImage = "Assets/turnleft.jpg"
                        },
                        new()
                        {
                            Ascent = 35,
                            Descent = 78,
                            Distance = 2.75,
                            Segment = "efgh",
                            SequenceNumber = 2,
                            TurnImage = "Assets/turnright.jpg"
                        },
                        new()
                        {
                            Ascent = 0,
                            Descent = 50,
                            Distance = 10.5,
                            Segment = "ijkl",
                            SequenceNumber = 1,
                            TurnImage = "Assets/gostraight.jpg"
                        }
                    }
                }
            };

            InitializeComponent();
        }

        private void SKElement_OnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            args.Surface.Canvas.Clear();
            
            // Lowest layer are the segments
            foreach (var skPath in _segmentPaths)
            {
                SKPaint segmentPaint;

                // Use a different color for the selected segment
                if (_selectedSegment != null && skPath.Key == _selectedSegment.Id)
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

            var segmentsWithOffsets = _segments
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
                _segmentPaths.Add(segment.Segment.Id, SkiaPathFromSegment(_overallOffsets, segment.GameCoordinates));
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
            var store = new SegmentStore();

            _segments = store.LoadSegments();

            CreatePathsForSegments();

            SkElement.InvalidateVisual();
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_segments != null)
            {
                CreatePathsForSegments();

                SkElement.InvalidateVisual();
            }
        }

        private void SkElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(sender as IInputElement);

            Debugger.Break();
        }
    }
}

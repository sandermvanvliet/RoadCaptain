using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using RoadCaptain.App.RouteBuilder.Models;
using RoadCaptain.App.RouteBuilder.ViewModels;
using SkiaSharp;

namespace RoadCaptain.App.RouteBuilder.Controls
{
    public class ZwiftMap : UserControl
    {
        private const float ZoomDelta = 0.1f;
        private readonly MapRenderOperation _renderOperation;
        private Point _previousPanPosition;
        private bool _isPanning;
        private Segment? _highlightedSegment;
        private Segment? _selectedSegment;
        private List<Segment>? _segments;

        public static readonly DirectProperty<ZwiftMap, bool> ShowClimbsProperty = AvaloniaProperty.RegisterDirect<ZwiftMap, bool>(nameof(ShowClimbs), map => map.ShowClimbs, (map, value) => map.ShowClimbs = value);
        public static readonly DirectProperty<ZwiftMap, bool> ShowSprintsProperty = AvaloniaProperty.RegisterDirect<ZwiftMap, bool>(nameof(ShowSprints), map => map.ShowSprints, (map, value) => map.ShowSprints = value);
        public static readonly DirectProperty<ZwiftMap, Segment?> HighlightedSegmentProperty = AvaloniaProperty.RegisterDirect<ZwiftMap, Segment?>(nameof(HighlightedSegment), map => map.HighlightedSegment, (map, value) => map.HighlightedSegment = value);
        public static readonly DirectProperty<ZwiftMap, Segment?> SelectedSegmentProperty = AvaloniaProperty.RegisterDirect<ZwiftMap, Segment?>(nameof(SelectedSegment), map => map.SelectedSegment, (map, value) => map.SelectedSegment = value);
        public static readonly DirectProperty<ZwiftMap, TrackPoint?> RiderPositionProperty = AvaloniaProperty.RegisterDirect<ZwiftMap, TrackPoint?>(nameof(RiderPosition), map => map.RiderPosition, (map, value) => map.RiderPosition = value);
        public static readonly DirectProperty<ZwiftMap, List<Segment>?> SegmentsProperty = AvaloniaProperty.RegisterDirect<ZwiftMap, List<Segment>?>(nameof(Segments), map => map.Segments, (map, value) => map.Segments = value);
        public static readonly DirectProperty<ZwiftMap, List<Segment>?> MarkersProperty = AvaloniaProperty.RegisterDirect<ZwiftMap, List<Segment>?>(nameof(Markers), map => map.Markers, (map, value) => map.Markers = value);
        public static readonly DirectProperty<ZwiftMap, RouteViewModel?> RouteProperty = AvaloniaProperty.RegisterDirect<ZwiftMap, RouteViewModel?>(nameof(Route), map => map.Route, (map, value) => map.Route = value);
        public static readonly DirectProperty<ZwiftMap, ICommand> SelectSegmentCommandProperty = AvaloniaProperty.RegisterDirect<ZwiftMap, ICommand>(nameof(SelectSegmentCommand), map => map.SelectSegmentCommand, (map, value) => map.SelectSegmentCommand = value);

        private Offsets? _overallOffsets;
        private readonly Dictionary<string, SKRect> _segmentPathBounds = new();
        private readonly Dictionary<string, SKPath> _segmentPaths = new();
        private RouteViewModel? _route;
        private List<Segment> _markers = new();
        private readonly Timer _closeTimer;
        private string? _toolTipIdentity;

        public ZwiftMap()
        {
            Background = new SolidColorBrush(Colors.Transparent);

            _renderOperation = new MapRenderOperation();

            _closeTimer = new Timer(state => Dispatcher.UIThread.InvokeAsync(() => ToolTip.SetIsOpen(this, false)),
                null,
                Timeout.InfiniteTimeSpan,
                Timeout.InfiniteTimeSpan);
        }

        public override void Render(DrawingContext context)
        {
            if (IsVisible)
            {
                context.Custom(_renderOperation);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // Take all the space we can get
            return availableSize;
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            if (change.Property.Name == nameof(Bounds))
            {
                // Always construct a new Rect without translation,
                // otherwise the rendering is offset _within_ the control
                // itself as the Bounds set on the control include the
                // left/top translation of the control to the parent (window).
                // For rendering we don't want that translation to happen
                // as we're drawing _inside_ of the control, not the parent.
                _renderOperation.Bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
            }

            base.OnPropertyChanged(change);
        }

        public Size CanvasSize => new((float)Bounds.Width, (float)Bounds.Height);

        public Segment? HighlightedSegment
        {
            get
            {
                if (_highlightedSegment?.Id != _renderOperation.HighlightedSegmentId)
                {
                    _renderOperation.HighlightedSegmentId = null;
                    _highlightedSegment = null;

                    InvalidateVisual();
                }

                return null;
            }
            set
            {
                _highlightedSegment = value;
                _renderOperation.HighlightedSegmentId = value?.Id;

                InvalidateVisual();
            }
        }

        public Segment? SelectedSegment
        {
            get
            {
                if (_selectedSegment?.Id != _renderOperation.SelectedSegmentId)
                {
                    _renderOperation.SelectedSegmentId = null;
                    _selectedSegment = null;

                    InvalidateVisual();
                }

                return null;
            }
            set
            {
                _selectedSegment = value;
                _renderOperation.SelectedSegmentId = value?.Id;

                InvalidateVisual();
            }
        }

        public bool ShowClimbs
        {
            get => _renderOperation.ShowClimbs;
            set
            {
                _renderOperation.ShowClimbs = value;

                InvalidateVisual();
            }
        }

        public bool ShowSprints
        {
            get => _renderOperation.ShowSprints;
            set
            {
                _renderOperation.ShowSprints = value;

                InvalidateVisual();
            }
        }

        public TrackPoint? RiderPosition
        {
            get => null;
            set
            {
                if(value != null)
                {
                    _renderOperation.RiderPosition = _segmentPaths[value.Segment.Id].Points[value.Index.Value];

                    InvalidateVisual();
                }
                else
                {
                    _renderOperation.RiderPosition = null;
                }
            }
        }

        public List<Segment>? Segments
        {
            get => _segments;
            set
            {
                _segments = value;

                if (_segments != null)
                {
                    CreatePathsForSegments(_segments, _renderOperation.Bounds);
                }

                InvalidateVisual();
            }
        }

        public List<Segment>? Markers
        {
            get => _markers;
            set
            {
                _markers = value ?? new List<Segment>();

                CreateMarkers();

                InvalidateVisual();
            }
        }

        public RouteViewModel? Route
        {
            get => _route;
            set
            {
                _route = value;
                _renderOperation.Route = value;

                CreateRoutePath();

                InvalidateVisual();
            }
        }

        public ICommand SelectSegmentCommand { get; set; }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            // Move pan
            var currentPoint = e.GetCurrentPoint(this);

            if (currentPoint.Properties.IsLeftButtonPressed && !_isPanning && _previousPanPosition == new Point(0, 0))
            {
                _isPanning = true;
                _previousPanPosition = currentPoint.Position;
            }

            if (_isPanning)
            {
                PanMove(currentPoint.Position);
                e.Handled = true;
                return;
            }
            
            var position = currentPoint.Position;

            // Check position against 0,0 because for some reason this event
            // gets triggered for that position _after_ we show the tool tip initially
            if (position != new Point(0,0) && (ShowSprints || ShowClimbs))
            {
                position = GetPositionOnCanvas(position);

                var matches = _renderOperation
                    .Markers
                    .Values
                    .Where(kv => kv.Bounds.Contains((float)position.X, (float)position.Y))
                    .ToList();

                if (matches.Count == 1)
                {
                    var marker = matches.Single();

                    // When the mouse moved to another marker and the tool tip
                    // is still open it doesn't reposition until it's closed.
                    // To ensure the tool tip doesn't hang around at the old
                    // position we close it here to have it re-open again at
                    // the right position.
                    if (_toolTipIdentity != marker.Id && ToolTip.GetIsOpen(this))
                    {
                        ToolTip.SetIsOpen(this, false);
                    }

                    _toolTipIdentity = marker.Id;

                    if ((marker.Type == SegmentType.Sprint && !ShowSprints) ||
                        (marker.Type == SegmentType.Climb && !ShowClimbs))
                    {
                        return;
                    }

                    ToolTip.SetTip(this, $"{marker.Type} {marker.Name}");

                    if (!ToolTip.GetIsOpen(this))
                    {
                        ToolTip.SetIsOpen(this, true);
                        ToolTip.SetPlacement(this, PlacementMode.Pointer);
                    }
                }
                else if (ToolTip.GetIsOpen(this))
                {
                    _closeTimer.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
                }
            }

            base.OnPointerMoved(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            // End pan
            if (_isPanning)
            {
                _isPanning = false;
                _previousPanPosition = new Point(0, 0);
                e.Handled = true;
                return;
            }

            var position = GetPositionOnCanvas(e.GetPosition(this));

            SelectSegment(position);

            base.OnPointerReleased(e);
        }

        private void SelectSegment(Point scaledPoint)
        {
            // Find SKPath that contains this coordinate (or close enough)
            var pathsInBounds = _segmentPathBounds
                .Where(p => p.Value.Contains((float)scaledPoint.X, (float)scaledPoint.Y))
                .OrderBy(x => x.Value, new SkRectComparer()) // Sort by bounds area, good enough for now
                .ToList();

            if (!pathsInBounds.Any())
            {
                return;
            }

            // Do expensive point to segment matching now that we've narrowed down the set
            var boundedSegments = pathsInBounds.Select(kv => Segments.Single(s => s.Id == kv.Key)).ToList();

            var reverseScaled = _overallOffsets.ReverseScaleAndTranslate(scaledPoint.X, scaledPoint.Y);
            var scaledPointToPosition = TrackPoint.FromGameLocation(reverseScaled.Latitude, reverseScaled.Longitude, reverseScaled.Altitude);
            scaledPointToPosition = new TrackPoint(-scaledPointToPosition.Longitude, scaledPointToPosition.Latitude, scaledPointToPosition.Altitude);

            Segment newSelectedSegment = null;

            foreach (var segment in boundedSegments)
            {
                if (segment.Contains(scaledPointToPosition))
                {
                    newSelectedSegment = segment;
                }
            }

            newSelectedSegment ??= boundedSegments.First();

            SelectSegmentCommand?.Execute(newSelectedSegment);
        }

        public void PanMove(Point position)
        {
            // When a drag operation is active,
            // track the delta-x and delta-y values
            // based on the start position of the
            // drag operation
            var newPanPosition = new Point(
                _renderOperation.Pan.X + (_previousPanPosition.X - position.X),
                _renderOperation.Pan.Y + (_previousPanPosition.Y - position.Y));

            _renderOperation.Pan = newPanPosition;
            _previousPanPosition = position;

            InvalidateVisual();
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            var position = e.GetPosition(this);

            if (e.Delta.Y > 0)
            {
                ZoomIn(position);
            }
            else if (e.Delta.Y < 0)
            {
                ZoomOut(position);
            }

            e.Handled = true;
        }

        public void ZoomIn(Point position)
        {
            _renderOperation.ZoomLevel += ZoomDelta;
            _renderOperation.ZoomCenter = position;

            InvalidateVisual();
        }

        public void ZoomOut(Point position)
        {
            if (Math.Abs(_renderOperation.ZoomLevel - 1) < 0.01)
            {
                return;
            }

            _renderOperation.ZoomLevel -= ZoomDelta;
            _renderOperation.ZoomCenter = position;

            InvalidateVisual();
        }

        public void ResetZoom()
        {
            _renderOperation.ZoomLevel = 1;
            _renderOperation.ZoomCenter = new Point(0, 0);
            _renderOperation.Pan = new Point(0, 0);

            InvalidateVisual();
        }

        public Point GetPositionOnCanvas(Point position)
        {
            var invertedLogical = _renderOperation.LogicalMatrix.Invert();

            if (_renderOperation.LogicalMatrix == SKMatrix.Empty)
            {
                return position;
            }

            var intermediate = invertedLogical.MapPoint((float)position.X, (float)position.Y);

            return new Point(intermediate.X, intermediate.Y);
        }

        private void CreatePathsForSegments(List<Segment> segments, Rect size)
        {
            _segmentPaths.Clear();
            _segmentPathBounds.Clear();

            if (!segments.Any())
            {
                return;
            }

            var segmentsWithOffsets = segments
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
                    Offsets = new Offsets((float)size.Width, (float)size.Height, x.GameCoordinates)
                })
                .ToList();

            _overallOffsets = Offsets
                .From(segmentsWithOffsets.Select(s => s.Offsets).ToList())
                .Pad(15);

            foreach (var segment in segmentsWithOffsets)
            {
                var skiaPathFromSegment = SkiaPathFromSegment(_overallOffsets, segment.GameCoordinates);
                skiaPathFromSegment.GetTightBounds(out var bounds);

                _segmentPaths.Add(segment.Segment.Id, skiaPathFromSegment);
                _segmentPathBounds.Add(segment.Segment.Id, bounds);
            }

            _renderOperation.SegmentPaths = _segmentPaths;
        }

        private void CreateMarkers()
        {
            var markers = new Dictionary<string, Marker>();

            markers.Clear();

            if (!_markers.Any())
            {
                return;
            }

            foreach (var segment in _markers.Where(m => m.Type == SegmentType.Climb || m.Type == SegmentType.Sprint))
            {
                var gameCoordinates = segment
                    .Points
                    .Select(point => TrackPoint.LatLongToGame(point.Longitude, -point.Latitude, point.Altitude))
                    .ToList();

                var startPoint = _overallOffsets.ScaleAndTranslate(gameCoordinates.First());
                var endPoint = _overallOffsets.ScaleAndTranslate(gameCoordinates.Last());

                var skiaPathFromSegment = SkiaPathFromSegment(_overallOffsets, gameCoordinates);
                skiaPathFromSegment.GetTightBounds(out var bounds);

                var marker = new Marker
                {
                    Id = segment.Id,
                    Name = segment.Name,
                    Type = segment.Type,
                    StartDrawPoint = new SKPoint(startPoint.X, startPoint.Y),
                    EndDrawPoint = new SKPoint(endPoint.X, endPoint.Y),
                    StartAngle = (float)TrackPoint.Bearing(segment.Points[0], segment.Points[1]) + 90,
                    EndAngle = (float)TrackPoint.Bearing(segment.Points[^2], segment.Points[^1]) + 90,
                    Path = skiaPathFromSegment,
                    Bounds = bounds,
                    StartPoint = segment.Points.First(),
                    EndPoint = segment.Points.Last()
                };

                markers.Add(segment.Id, marker);
            }

            _renderOperation.Markers = markers;
        }

        private static SKPath SkiaPathFromSegment(Offsets offsets, List<TrackPoint> data)
        {
            var path = new SKPath();

            path.AddPoly(
                data
                    .Select(offsets.ScaleAndTranslate)
                    .Select(point => new SKPoint(point.X, point.Y))
                    .ToArray(),
                false);

            return path;
        }

        private void CreateRoutePath()
        {
            var routePath = new SKPath();

            if (Route == null)
            {
                _renderOperation.RoutePath = routePath;
                return;
            }

            // RoutePath needs to be set to the total route we just loaded
            foreach (var segment in Route.Sequence)
            {
                var points = _segmentPaths[segment.SegmentId].Points;

                if (segment.Direction == SegmentDirection.BtoA)
                {
                    points = points.Reverse().ToArray();
                }

                routePath.AddPoly(points, false);
            }

            _renderOperation.RoutePath = routePath;
        }
    }
}

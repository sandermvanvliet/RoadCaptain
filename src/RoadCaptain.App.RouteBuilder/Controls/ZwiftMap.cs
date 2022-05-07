using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using RoadCaptain.App.RouteBuilder.ViewModels;

namespace RoadCaptain.App.RouteBuilder.Controls
{
    public class ZwiftMap : UserControl
    {
        private const float ZoomDelta = 0.1f;
        private readonly MapRenderOperation _renderOperation;
        private Point _previousPanPosition;
        private bool _isPanning;
        private Segment? _highlightedSegment;
        private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;
        
        public static readonly DirectProperty<ZwiftMap, bool> ShowClimbsProperty = AvaloniaProperty.RegisterDirect<ZwiftMap, bool>(nameof(ShowClimbs), map =>  map.ShowClimbs, (map,value) => map.ShowClimbs = value);
        public static readonly DirectProperty<ZwiftMap, bool> ShowSprintsProperty = AvaloniaProperty.RegisterDirect<ZwiftMap, bool>(nameof(ShowSprints), map =>  map.ShowSprints, (map,value) => map.ShowSprints = value);
        public static readonly DirectProperty<ZwiftMap, Segment?> HighlightedSegmentProperty = AvaloniaProperty.RegisterDirect<ZwiftMap, Segment?>(nameof(HighlightedSegment), map =>  map.HighlightedSegment, (map,value) => map.HighlightedSegment = value);

        public ZwiftMap()
        {
            ClipToBounds = true;
            IsHitTestVisible = true;
            Background = new SolidColorBrush(Colors.Transparent);

            _renderOperation = new MapRenderOperation();
        }

        public override void Render(DrawingContext context)
        {
            if (ViewModel == null)
            {
                return;
            }

            if (_renderOperation.ViewModel == null)
            {
                _renderOperation.ViewModel = ViewModel;
            }

            context.Custom(_renderOperation);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // Take all the space we can get
            return availableSize;
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            if (change.Property.Name == nameof(Bounds) && ViewModel != null)
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
                if(_highlightedSegment?.Id != _renderOperation.HighlightedSegmentId)
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

            base.OnPointerReleased(e);
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
    }
}

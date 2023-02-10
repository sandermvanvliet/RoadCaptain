// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;

namespace RoadCaptain.App.Shared.Controls
{
    public class ElevationProfile : UserControl
    {
        private readonly ElevationProfileRenderOperation _renderOperation;
        public static readonly DirectProperty<ElevationProfile, PlannedRoute?> RouteProperty = AvaloniaProperty.RegisterDirect<ElevationProfile, PlannedRoute?>(nameof(Route), map => map.Route, (map, value) => map.Route = value);
        public static readonly DirectProperty<ElevationProfile, List<Segment>?> SegmentsProperty = AvaloniaProperty.RegisterDirect<ElevationProfile, List<Segment>?>(nameof(Segments), map => map.Segments, (map, value) => map.Segments = value);
        public static readonly DirectProperty<ElevationProfile, TrackPoint?> RiderPositionProperty = AvaloniaProperty.RegisterDirect<ElevationProfile, TrackPoint?>(nameof(RiderPosition), map => map.RiderPosition, (map, value) => map.RiderPosition = value);
        public static readonly DirectProperty<ElevationProfile, List<Segment>?> MarkersProperty = AvaloniaProperty.RegisterDirect<ElevationProfile, List<Segment>?>(nameof(Markers), map => map.Markers, (map, value) => map.Markers = value);
        public static readonly DirectProperty<ElevationProfile, bool> ShowClimbsProperty = AvaloniaProperty.RegisterDirect<ElevationProfile, bool>(nameof(ShowClimbs), map => map.ShowClimbs, (map, value) => map.ShowClimbs = value);
        public static readonly DirectProperty<ElevationProfile, bool> ZoomOnCurrentPositionProperty = AvaloniaProperty.RegisterDirect<ElevationProfile, bool>(nameof(ZoomOnCurrentPosition), map => map.ZoomOnCurrentPosition, (map, value) => map.ZoomOnCurrentPosition = value);
        public static readonly DirectProperty<ElevationProfile, int> ZoomWindowDistanceProperty = AvaloniaProperty.RegisterDirect<ElevationProfile, int>(nameof(ZoomWindowDistance), map => map.ZoomWindowDistance, (map, value) => map.ZoomWindowDistance = value);
        
        private RenderTargetBitmap? _renderTarget;
        private ISkiaDrawingContextImpl? _skiaContext;

        public PlannedRoute? Route
        {
            get => _renderOperation.Route;
            set
            {
                _renderOperation.Route = value;

                InvalidateVisual();
            }
        }

        public List<Segment>? Segments
        {
            get => _renderOperation.Segments;
            set
            {
                _renderOperation.Segments = value;

                InvalidateVisual();
            }
        }

        public TrackPoint? RiderPosition
        {
            get => null;
            set
            {
                if(value is { Index: { } })
                {
                    _renderOperation.RiderPosition = value;

                    InvalidateVisual();
                }
                else
                {
                    _renderOperation.RiderPosition = null;
                }
            }
        }

        public List<Segment>? Markers
        {
            get => _renderOperation.Markers;
            set
            {
                _renderOperation.Markers = value ?? new List<Segment>();

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

        public bool ZoomOnCurrentPosition
        {
            get => _renderOperation.ZoomOnCurrentPosition;
            set
            {
                _renderOperation.ZoomOnCurrentPosition = value;

                InvalidateVisual();
            }
        }

        public int ZoomWindowDistance
        {
            get => _renderOperation.ZoomWindowDistance;
            set
            {
                _renderOperation.ZoomWindowDistance = value;
                
                InvalidateVisual();
            }
        }

        public ElevationProfile()
        {
            Background = new SolidColorBrush(Colors.Transparent);

            _renderOperation = new ElevationProfileRenderOperation();
        }

        public override void Render(DrawingContext context)
        {
            if (IsVisible)
            {
                context.Custom(_renderOperation);
            }

            if (_renderTarget != null)
            {
                RenderElevationProfile();

                context
                    .DrawImage(
                        _renderTarget,
                        new Rect(0, 0, _renderTarget.PixelSize.Width, _renderTarget.PixelSize.Height),
                        new Rect(0, 0, Bounds.Width, Bounds.Height));
            }
        }

        private void InitializeRenderTarget()
        {
            _renderTarget = new RenderTargetBitmap(new PixelSize((int)Bounds.Width, (int)Bounds.Height));
            var context = _renderTarget.CreateDrawingContext(null);
            _skiaContext = context as ISkiaDrawingContextImpl;
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

                InitializeRenderTarget();

                InvalidateVisual();
            }

            base.OnPropertyChanged(change);
        }

        private void RenderElevationProfile()
        {
            if (_skiaContext != null)
            {
                _renderOperation.Render(_skiaContext);
            }
        }
    }
}

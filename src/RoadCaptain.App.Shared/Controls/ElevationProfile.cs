// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;

namespace RoadCaptain.App.Shared.Controls
{
    public class ElevationProfile : UserControl
    {
        private readonly ElevationProfileLayeredRenderOperation _renderOperation;
        public static readonly DirectProperty<ElevationProfile, PlannedRoute?> RouteProperty = AvaloniaProperty.RegisterDirect<ElevationProfile, PlannedRoute?>(nameof(Route), map => map.Route, (map, value) => map.Route = value);
        public static readonly DirectProperty<ElevationProfile, List<Segment>?> SegmentsProperty = AvaloniaProperty.RegisterDirect<ElevationProfile, List<Segment>?>(nameof(Segments), map => map.Segments, (map, value) => map.Segments = value);
        public static readonly DirectProperty<ElevationProfile, TrackPoint?> RiderPositionProperty = AvaloniaProperty.RegisterDirect<ElevationProfile, TrackPoint?>(nameof(RiderPosition), map => map.RiderPosition, (map, value) => map.RiderPosition = value);
        public static readonly DirectProperty<ElevationProfile, List<Segment>?> MarkersProperty = AvaloniaProperty.RegisterDirect<ElevationProfile, List<Segment>?>(nameof(Markers), map => map.Markers, (map, value) => map.Markers = value);
        public static readonly DirectProperty<ElevationProfile, RenderMode> RenderModeProperty = AvaloniaProperty.RegisterDirect<ElevationProfile, RenderMode>(nameof(RenderMode), map => map.RenderMode, (map, value) => map.RenderMode = value);
        
        private RenderTargetBitmap? _renderTarget;
        private DrawingContext _drawingContext;

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
                if(value is { Index: not null })
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
                _renderOperation.Markers = (value ?? new List<Segment>())
                    .Where(m => m.Type == SegmentType.Climb || m.Type == SegmentType.Sprint)
                    .ToList();

                InvalidateVisual();
            }
        }

        public RenderMode RenderMode
        {
            get => _renderOperation.RenderMode;
            set
            {
                if (value == _renderOperation.RenderMode) return;
                
                _renderOperation.RenderMode = value;
                
                InvalidateVisual();
            }
        }

        public ElevationProfile()
        {
            Background = new SolidColorBrush(Colors.Transparent);

            _renderOperation = new ElevationProfileLayeredRenderOperation();
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

            _drawingContext = _renderTarget.CreateDrawingContext();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // Take all the space we can get
            return availableSize;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
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
            // if (_drawingContext != null)
            // {
            //     _renderOperation.Render(_drawingContext);
            // }
        }
    }
}

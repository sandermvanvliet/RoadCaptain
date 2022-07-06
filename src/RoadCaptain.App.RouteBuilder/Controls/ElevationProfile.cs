using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using RoadCaptain.App.RouteBuilder.ViewModels;

namespace RoadCaptain.App.RouteBuilder.Controls
{
    public class ElevationProfile : UserControl
    {
        private readonly ElevationProfileRenderOperation _renderOperation;
        public static readonly DirectProperty<ElevationProfile, RouteViewModel?> RouteProperty = AvaloniaProperty.RegisterDirect<ElevationProfile, RouteViewModel?>(nameof(Route), map => map.Route, (map, value) => map.Route = value);
        public static readonly DirectProperty<ElevationProfile, List<Segment>?> SegmentsProperty = AvaloniaProperty.RegisterDirect<ElevationProfile, List<Segment>?>(nameof(Segments), map => map.Segments, (map, value) => map.Segments = value);
        private RouteViewModel _route;
        
        public RouteViewModel? Route
        {
            get => _route;
            set
            {
                _route = value;
                _renderOperation.Route = value;

                InvalidateVisual();
            }
        }

        public List<Segment?>? Segments
        {
            get => _renderOperation.Segments;
            set
            {
                _renderOperation.Segments = value;

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
                InvalidateVisual();
                // TODO: make it work
                //if (Segments != null && Bounds.Width > 0 && Bounds.Height > 0)
                //{
                //    CreatePathsForSegments(Segments, Bounds);
                //}
            }

            base.OnPropertyChanged(change);
        }
    }
}
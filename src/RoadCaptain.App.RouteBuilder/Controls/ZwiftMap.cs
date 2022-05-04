using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using RoadCaptain.App.RouteBuilder.ViewModels;

namespace RoadCaptain.App.RouteBuilder.Controls
{
    public class ZwiftMap : UserControl 
    {
        private CanvasRenderOperation? _customDrawOp;
        private readonly MainWindowViewModel _mainWindowViewModel;

        public ZwiftMap()
        {
            ClipToBounds = true;
            IsHitTestVisible = true;
            Background = new SolidColorBrush(Colors.Transparent);
            
            _mainWindowViewModel = DataContext as MainWindowViewModel ?? throw new ArgumentException($"{nameof(DataContext)} value must be a {nameof(MainWindowViewModel)}");
        }

        public override void Render(DrawingContext context)
        {
            if (_customDrawOp == null)
            {
                _customDrawOp = new CanvasRenderOperation(
                    new Rect(0, 0, Bounds.Width, Bounds.Height),
                    _mainWindowViewModel);
            }

            context.Custom(_customDrawOp);

            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
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
                _customDrawOp = new CanvasRenderOperation(
                    new Rect(0, 0, Bounds.Width, Bounds.Height),
                    _mainWindowViewModel);

                Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
            }

            base.OnPropertyChanged(change);
        }

        public Size CanvasSize => new((float)Bounds.Width, (float)Bounds.Height);

        public string? HighlightedSegmentId
        {
            get => _customDrawOp?.HighlightedSegmentId;
            set
            {
                if (_customDrawOp != null)
                {
                    _customDrawOp.HighlightedSegmentId = value;
                }
            }
        }
    }
}

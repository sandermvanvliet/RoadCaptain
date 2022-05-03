using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using RoadCaptain.App.RouteBuilder.ViewModels;
using SkiaSharp;

namespace RoadCaptain.App.RouteBuilder.Controls
{
    public class SkiaCanvas : UserControl 
    {
        private CustomDrawOp? _customDrawOp;

        public SkiaCanvas()
        {
            ClipToBounds = true;
            IsHitTestVisible = true;
            Background = new SolidColorBrush(Colors.Transparent);
        }

        public override void Render(DrawingContext context)
        {
            var noSkia = new FormattedText()
            {
                Text = "Current rendering API is not Skia"
            };

            if (_customDrawOp == null)
            {
                _customDrawOp = new CustomDrawOp(
                    new Rect(0, 0, Bounds.Width, Bounds.Height), 
                    noSkia,
                    DataContext as MainWindowViewModel);
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
                Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
            }

            base.OnPropertyChanged(change);
        }

        public Size CanvasSize => new((float)Bounds.Width, (float)Bounds.Height);

        public SKMatrix CurrentMatrix => _customDrawOp.CurrentMatrix;

        public string? HighlightedSegmentId
        {
            get => _customDrawOp.HighlightedSegmentId;
            set => _customDrawOp.HighlightedSegmentId = value;
        }
    }
}

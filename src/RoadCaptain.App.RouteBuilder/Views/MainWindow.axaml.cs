using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using RoadCaptain.App.RouteBuilder.Controls;
using RoadCaptain.App.RouteBuilder.ViewModels;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public partial class MainWindow : Window
    {
        // ReSharper disable once UnusedMember.Global because this constructor only exists for the Avalonia designer
#pragma warning disable CS8618
        public MainWindow()
#pragma warning restore CS8618
        {
        }

        public MainWindow(MainWindowViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.PropertyChanged += WindowViewModelPropertyChanged;
            DataContext = viewModel;

            InitializeComponent();

            SkElement
                .PropertyChanged += (sender, args) =>
            {
                if (args.Property.Name == "Bounds")
                {
                    if (sender is SkiaCanvas skiaCanvas && !double.IsNaN(skiaCanvas.CanvasSize.Width) && !double.IsNaN(skiaCanvas.CanvasSize.Height))
                    {
                        ViewModel.CreatePathsForSegments((float)skiaCanvas.CanvasSize.Width, (float)skiaCanvas.CanvasSize.Height);
                        TriggerRepaint();
                    }
                }
            };
        }

        private MainWindowViewModel ViewModel { get; }

        private void WindowViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // TODO: Use reactive approach with WhenXxx() for this
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SelectedSegment):
                case nameof(ViewModel.SegmentPaths):
                    // Reset any manually selected item in the list
                    SkElement.HighlightedSegmentId = null;
                    TriggerRepaint();
                    break;
                case nameof(ViewModel.Route):
                    // Ensure the last added segment is visible
                    if (RouteListView.ItemCount > 0)
                    {
                        RouteListView.ScrollIntoView(RouteListView.ItemCount - 1);
                    }

                    // When a world is selected the path segments
                    // need to be generated which needs the canvas
                    // size. Therefore we need to call that from
                    // this handler
                    if (ViewModel.Route.World != null && !ViewModel.SegmentPaths.Any())
                    {
                        ViewModel.CreatePathsForSegments((float)SkElement.CanvasSize.Width, (float)SkElement.CanvasSize.Height);
                    }

                    // Redraw when the route changes so that the
                    // route path is painted correctly
                    TriggerRepaint();
                    break;
                case nameof(ViewModel.RiderPosition):
                case nameof(ViewModel.ShowClimbs):
                case nameof(ViewModel.ShowSprints):
                case nameof(ViewModel.Zoom):
                case nameof(ViewModel.Pan):
                    TriggerRepaint();
                    break;
            }
        }


        private void TriggerRepaint()
        {
            if (SkElement.CheckAccess())
            {
                SkElement.InvalidateVisual();
            }
            else
            {
                Dispatcher.UIThread.InvokeAsync(TriggerRepaint);
            }
        }

        private void SkElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(sender as IVisual);

            if (currentPoint.Properties.IsLeftButtonPressed)
            {
                ViewModel.StartPan(currentPoint.Position);
            }
        }

        private void SkElement_OnPointerMoved(object? sender, PointerEventArgs e)
        {
            var position = e.GetPosition((IInputElement?)sender);

            if (!ViewModel.IsPanning)
            {
                return;
            }

            if (ViewModel.IsPanning)
            {
                ViewModel.PanMove(position);

                return;
            }

            // Hit test to see whether we're over a KOM/Sprint segment

            // If sprints and climbs are not shown then exit
            if (!ViewModel.ShowSprints && !ViewModel.ShowClimbs)
            {
                return;
            }

            var matches = ViewModel
                .Markers
                .Values
                .Where(kv => kv.Bounds.Contains((float)position.X, (float)position.Y))
                .ToList();

            if (matches.Count == 1)
            {
                var marker = matches.Single();

                ViewModel.Model.StatusBarInfo("{0} {1}", marker.Type.ToString(), marker.Name);
            }
            else
            {
                ViewModel.Model.ClearStatusBar();
            }
        }

        private void SkElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (sender is not SkiaCanvas skiaCanvas)
            {
                return;
            }

            if (ViewModel.IsPanning)
            {
                ViewModel.EndPan();
                return;
            }

            var position = e.GetPosition((IInputElement)sender);

            ViewModel.SelectSegmentCommand.Execute(position);
        }

        private void RouteListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox { SelectedItem: SegmentSequenceViewModel viewModel })
            {
                SkElement.HighlightedSegmentId = viewModel.SegmentId;
                TriggerRepaint();
            }
            else
            {
                SkElement.HighlightedSegmentId = null;
            }
        }

        private void RouteListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender is ListBox { SelectedItem: SegmentSequenceViewModel viewModel } && e.Key == Key.Delete)
            {
                if (viewModel == ViewModel.Route.Last)
                {
                    ViewModel.RemoveLastSegmentCommand.Execute(null);
                    if (RouteListView.ItemCount > 0)
                    {
                        RouteListView.SelectedItem = RouteListView.Items.Cast<object>().Last();
                    }
                }
            }
        }

        private void MainWindow_OnActivated(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => ViewModel.CheckForNewVersion());
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ZoomIn(new Point(SkElement.Bounds.Width / 2, SkElement.Bounds.Height / 2));

            TriggerRepaint();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ZoomOut(new Point(SkElement.Bounds.Width / 2, SkElement.Bounds.Height / 2));

            TriggerRepaint();
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ResetZoomAndPan();

            TriggerRepaint();
        }

        private void SkElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            var position = e.GetPosition((IInputElement)sender);

            var canvasCoordinate = position;

            if (e.Delta.Y > 0)
            {
                ViewModel.ZoomIn(canvasCoordinate);
            }
            else if (e.Delta.Y < 0)
            {
                ViewModel.ZoomOut(canvasCoordinate);
            }
        }
    }
}
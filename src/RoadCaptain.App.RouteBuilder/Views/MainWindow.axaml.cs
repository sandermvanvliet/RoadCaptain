using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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

            KeyBindings.Add(new KeyBinding{ Command = ViewModel.OpenRouteCommand, Gesture = new KeyGesture(Key.O, KeyModifiers.Control)});
            KeyBindings.Add(new KeyBinding{ Command = ViewModel.SaveRouteCommand, Gesture = new KeyGesture(Key.S, KeyModifiers.Control)});
            KeyBindings.Add(new KeyBinding{ Command = ViewModel.ClearRouteCommand, Gesture = new KeyGesture(Key.R, KeyModifiers.Control)});
            KeyBindings.Add(new KeyBinding{ Command = ViewModel.RemoveLastSegmentCommand, Gesture = new KeyGesture(Key.Z, KeyModifiers.Control)});
        }

        private MainWindowViewModel ViewModel { get; }

        private void WindowViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SelectedSegment):
                case nameof(ViewModel.SegmentPaths):
                    // Reset any manually selected item in the list
                    ViewModel.ClearSegmentHighlight();
                    break;
                case nameof(ViewModel.Route):
                    // Ensure the last added segment is visible
                    if (RouteListView.ItemCount > 0)
                    {
                        RouteListView.ScrollIntoView(RouteListView.ItemCount - 1);
                    }

                    // Redraw when the route changes so that the
                    // route path is painted correctly
                    break;
            }
        }
        
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void RouteListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1 && e.AddedItems[0] is SegmentSequenceViewModel viewModel)
            {
                ViewModel.HighlightSegment(viewModel.SegmentId);
            }
            else
            {
                ViewModel.ClearSegmentHighlight();
            }
        }

        // ReSharper disable once UnusedMember.Local
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
        
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void MainWindow_OnActivated(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => ViewModel.CheckForNewVersion());
        }

        // ReSharper disable once UnusedMember.Local
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            SkElement.ZoomIn(new Point(SkElement.Bounds.Width / 2, SkElement.Bounds.Height / 2));
        }
        
        // ReSharper disable once UnusedMember.Local
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            SkElement.ZoomOut(new Point(SkElement.Bounds.Width / 2, SkElement.Bounds.Height / 2));
        }

        // ReSharper disable once UnusedMember.Local
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            SkElement.ResetZoom();
        }
    }
}
using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly ISegmentStore _segmentStore;

        // ReSharper disable once UnusedMember.Global because this constructor only exists for the Avalonia designer
#pragma warning disable CS8618
        public MainWindow()
#pragma warning restore CS8618
        {
            InitializeComponent();
        }

        public MainWindow(MainWindowViewModel viewModel, IGameStateReceiver gameStateReceiver, ISegmentStore segmentStore)
        {
            _viewModel = viewModel;
            _segmentStore = segmentStore;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.RoutePath) && !string.IsNullOrEmpty(_viewModel.RoutePath))
                {
                    RebelRouteCombo.SelectedItem = null;
                }
            };

            gameStateReceiver.Register(route => viewModel.Route = RouteModel.From(route, _segmentStore.LoadSegments(route.World, route.Sport)), null, viewModel.UpdateGameState);

            DataContext = viewModel;

            InitializeComponent();
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Selector_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;

            if (comboBox?.SelectedItem != null)
            {
                _viewModel.RoutePath = null;

                if (comboBox.SelectedItem is PlannedRoute selectedRoute)
                {
                    _viewModel.Route = RouteModel.From(selectedRoute, _segmentStore.LoadSegments(selectedRoute.World, selectedRoute.Sport));
                }
                else
                {
                    _viewModel.Route = new RouteModel();
                }
            }
        }

        private void WindowBase_OnActivated(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() => _viewModel.CheckForNewVersion());
        }

        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }
    }
}

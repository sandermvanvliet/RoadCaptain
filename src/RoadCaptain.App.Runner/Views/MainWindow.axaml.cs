// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Avalonia;
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
        private bool _isFirstTimeActivation = true;

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

            gameStateReceiver.ReceiveRoute(route => viewModel.Route = RouteModel.From(route, _segmentStore.LoadSegments(route.World, route.Sport), _segmentStore.LoadMarkers(route.World)));
            gameStateReceiver.ReceiveGameState(viewModel.UpdateGameState);

            DataContext = viewModel;

            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
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
                    _viewModel.Route = RouteModel.From(selectedRoute, _segmentStore.LoadSegments(selectedRoute.World, selectedRoute.Sport), _segmentStore.LoadMarkers(selectedRoute.World));
                }
                else
                {
                    _viewModel.Route = new RouteModel();
                }
            }
        }

        private void WindowBase_OnActivated(object? sender, EventArgs e)
        {
            if(_isFirstTimeActivation)
            {
                _isFirstTimeActivation = false;

                Dispatcher.UIThread.InvokeAsync(() => _viewModel.Initialize());
                Dispatcher.UIThread.InvokeAsync(() => _viewModel.CheckForNewVersion());
                Dispatcher.UIThread.InvokeAsync(() => _viewModel.CheckLastOpenedVersion());
            }
        }

        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(this);

            if (currentPoint.Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void RebelRouteCombo_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // This prevents the situation where the PointerPressed event bubbles
            // up to the window and initiates the window drag operation.
            // It fixes a bug where the combo box can't be opened.
            e.Handled = true;
        }
    }
}


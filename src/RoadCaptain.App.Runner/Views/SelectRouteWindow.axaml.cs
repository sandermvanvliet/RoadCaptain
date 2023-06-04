// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using RoadCaptain.App.Runner.ViewModels;
using Serilog.Core;

namespace RoadCaptain.App.Runner.Views
{
    public partial class SelectRouteWindow : Window
    {
        private readonly SelectRouteWindowViewModel _viewModel;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IWindowService _windowService;

        // ReSharper disable once UnusedMember.Global because this constructor only exists for the Avalonia designer
#pragma warning disable CS8618
        public SelectRouteWindow()
#pragma warning restore CS8618
        {
            _monitoringEvents = new MonitoringEventsWithSerilog(Logger.None);
            _windowService = new DesignTimeWindowService();
            InitializeComponent();
        }


        public SelectRouteWindow(
            SelectRouteWindowViewModel viewModel, 
            MonitoringEvents monitoringEvents,
            IWindowService windowService)
        {
            _viewModel = viewModel;
            _monitoringEvents = monitoringEvents;
            _windowService = windowService;

            DataContext = viewModel;

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public RouteModel? SelectedRoute => _viewModel.SelectedRoute?.AsRouteModel();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WindowBase_OnActivated(object? sender, EventArgs e)
        {
            // Remove event handler to ensure this is only called once
            Activated -= WindowBase_OnActivated;

            Dispatcher.UIThread.InvokeAsync(() => _viewModel.Initialize());
        }

        private void RoutesListBox_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // This prevents the situation where the PointerPressed event bubbles
            // up to the window and initiates the window drag operation.
            // It fixes a bug where the combo box can't be opened.
            e.Handled = true;
        }

        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(this);

            if (currentPoint.Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void RoutesListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox)
            {
                return;
            }

            _viewModel.SelectedRoute = listBox.SelectedItem as RouteViewModel;
        }

        private async void RepositoryComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox comboBox)
            {
                return;
            }

            if (comboBox.SelectedItem is not string repositoryName)
            {
                return;
            }

            _viewModel.SearchRoutesCommand.Execute(repositoryName);
        }
    }
}

// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.Commands;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public partial class MainWindow : Window
    {
        private readonly IWindowService _windowService;
        private readonly ViewLocator _viewLocator;

        // Note: This constructor is used by the Avalonia designer only
        // ReSharper disable once UnusedMember.Global because this constructor only exists for the Avalonia designer
#pragma warning disable CS8618
        public MainWindow()
#pragma warning restore CS8618
        {
            ViewModel = (DataContext as MainWindowViewModel)!; // Suppressed because it's initialized from XAML

            InitializeComponent();
        }

        public MainWindow(MainWindowViewModel viewModel, IUserPreferences userPreferences, IWindowService windowService, ViewLocator viewLocator)
        {
            InitializeComponent();
            
            _windowService = windowService;
            _viewLocator = viewLocator;

            this.UseWindowStateTracking(
                userPreferences.RouteBuilderLocation,
                newWindowLocation =>
                {
                    userPreferences.RouteBuilderLocation = newWindowLocation;
                    userPreferences.Save();
                });

            ViewModel = viewModel;
            DataContext = viewModel;
            
            this.Bind(ViewModel.SaveRouteCommand).To(Key.S).WithPlatformModifier();
            this.Bind(ViewModel.ClearRouteCommand).To(Key.R).WithPlatformModifier();
            this.Bind(ViewModel.RemoveLastSegmentCommand).To(Key.Z).WithPlatformModifier();
        }

        private MainWindowViewModel ViewModel { get; }

        private void MainWindow_OnActivated(object? sender, EventArgs e)
        {
            // Remove event handler to ensure this is only called once
            Activated -= MainWindow_OnActivated;

            Dispatcher.UIThread.InvokeAsync(() => ViewModel.CheckForNewVersion());
            Dispatcher.UIThread.InvokeAsync(() => ViewModel.CheckLastOpenedVersion());
            Dispatcher.UIThread.InvokeAsync(() => ViewModel.LandingPageViewModel.LoadMyRoutesCommand.Execute(null));
        }

        private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
        {
            //_windowService.Shutdown(0);
        }
    }
}
// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.RouteBuilder.Views;
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.App.Shared.Dialogs.ViewModels;
using IApplicationLifetime = Avalonia.Controls.ApplicationLifetimes.IApplicationLifetime;

namespace RoadCaptain.App.RouteBuilder
{
    public class WindowService : BaseWindowService, IWindowService
    {
        private IClassicDesktopStyleApplicationLifetime _applicationLifetime;

        public WindowService(IComponentContext componentContext, MonitoringEvents monitoringEvents) : base(componentContext, monitoringEvents)
        {
        }

        public async Task ShowAlreadyRunningDialog()
        {
            await MessageBox.ShowAsync(
                "Only one instance of RoadCaptain Route Builder can be active",
                "Already running",
                MessageBoxButton.Ok,
                CurrentWindow,
                MessageBoxIcon.Warning);
        }

        public void SetLifetime(IApplicationLifetime applicationLifetime)
        {
            _applicationLifetime = applicationLifetime as IClassicDesktopStyleApplicationLifetime;
        }

        public async Task<string?> ShowSaveFileDialog(string? previousLocation, string? suggestedFileName = null)
        {
            var initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (!string.IsNullOrEmpty(previousLocation) && Directory.Exists(previousLocation))
            {
                initialDirectory = previousLocation;
            }

            var dialog = new SaveFileDialog
            {
                Directory = initialDirectory,
                DefaultExtension = "*.json",
                Filters = new List<FileDialogFilter>
                {
                    new() { Extensions = new List<string>{"json"}, Name = "RoadCaptain route file (.json)"},
                    new() { Extensions = new List<string>{"gpx"}, Name = "GPS Exchange Format (.gpx)"}
                },
                Title = "Save RoadCaptain route file",
                InitialFileName = suggestedFileName
            };

            return await dialog.ShowAsync(CurrentWindow);
        }

        public async Task<bool> ShowDefaultSportSelectionDialog(SportType sport)
        {
            var result = await MessageBox.ShowAsync(
                $"Do you want to use {sport} as your default selection?\nIf you do you won't have to select it again when you build more routes.",
                "Select default sport",
                MessageBoxButton.YesNo,
                CurrentWindow,
                MessageBoxIcon.Question);

            return result == MessageBoxResult.Yes;
        }

        public async Task<MessageBoxResult> ShowShouldSaveRouteDialog()
        {
            return await MessageBox.ShowAsync(
                "Do you want to save the current route?",
                "Current route was changed",
                MessageBoxButton.YesNoCancel,
                CurrentWindow);
        }

        public async Task<MessageBoxResult> ShowClearRouteDialog()
        {
            return await MessageBox.ShowAsync(
                    "This action will remove all segments from the current route. Are you sure?",
                    "Clear route",
                    MessageBoxButton.YesNo,
                    CurrentWindow,
                    MessageBoxIcon.Question);
        }

        public async Task<bool> ShowRouteLoopDialog()
        {
            var result = await MessageBox.ShowAsync(
                "The route ends on a connection to the first segment, do you want to make it a loop?",
                "Create route loop",
                MessageBoxButton.YesNo,
                CurrentWindow,
                MessageBoxIcon.Question);

            return result == MessageBoxResult.Yes;
        }

        public async Task<string?> ShowSaveRouteDialog(string? lastUsedFolder, RouteViewModel routeViewModel)
        {
            var saveRouteDialog = Resolve<SaveRouteDialog>();

            var viewModel = new SaveRouteDialogViewModel(this, Resolve<IUserPreferences>())
            {
                Route = routeViewModel
            };

            saveRouteDialog.DataContext = viewModel;

            await saveRouteDialog.ShowDialog(CurrentWindow);

            return viewModel.Path;
        }

        public void Shutdown(int exitCode)
        {
            _applicationLifetime.Shutdown(exitCode);
        }

        public void ShowMainWindow(IApplicationLifetime applicationLifetime)
        {
            var desktopMainWindow = Resolve<MainWindow>();

            if (applicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow == null)
            {
                desktop.MainWindow = desktopMainWindow;
            }

            base.Show(desktopMainWindow);
        }
    }
}

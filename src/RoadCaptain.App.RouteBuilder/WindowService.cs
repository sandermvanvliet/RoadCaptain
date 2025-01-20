// Copyright (c) 2023 Sander van Vliet
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
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.App.Shared.Dialogs.ViewModels;
using RoadCaptain.App.Shared.Models;
using RoadCaptain.App.Shared.Views;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;
using RouteViewModel = RoadCaptain.App.RouteBuilder.ViewModels.RouteViewModel;

namespace RoadCaptain.App.RouteBuilder
{
    public class WindowService : BaseWindowService, IWindowService
    {
        public WindowService(IComponentContext componentContext, MonitoringEvents monitoringEvents) : base(
            componentContext, monitoringEvents)
        {
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
                    new() { Extensions = new List<string> { "json" }, Name = "RoadCaptain route file (.json)" },
                    new() { Extensions = new List<string> { "gpx" }, Name = "GPS Exchange Format (.gpx)" }
                },
                Title = "Save RoadCaptain route file",
                InitialFileName = suggestedFileName
            };

            return await dialog.ShowAsync(CurrentWindow!);
        }

        public async Task<bool> ShowDefaultSportSelectionDialog(SportType sport)
        {
            var result = await MessageBox.ShowAsync(
                $"Do you want to use {sport} as your default selection?\nIf you do you won't have to select it again when you build more routes.",
                "Select default sport",
                MessageBoxButton.YesNo,
                CurrentWindow!,
                MessageBoxIcon.Question);

            return result == MessageBoxResult.Yes;
        }

        public async Task<MessageBoxResult> ShowShouldSaveRouteDialog()
        {
            return await MessageBox.ShowAsync(
                "Do you want to save the current route?",
                "Current route was changed",
                MessageBoxButton.YesNoCancel,
                CurrentWindow!);
        }

        public async Task<MessageBoxResult> ShowClearRouteDialog()
        {
            return await MessageBox.ShowAsync(
                "This action will remove all segments from the current route. Are you sure?",
                "Clear route",
                MessageBoxButton.YesNo,
                CurrentWindow!,
                MessageBoxIcon.Question);
        }

        public async Task<(bool Success, LoopMode Mode, int? NumberOfLoops)> ShowRouteLoopDialog(LoopMode? loopMode = null,
            int? numberOfLoops = null)
        {
            var makeLoopDialog = Resolve<MakeLoopDialog>();
            var makeLoopDialogViewModel = new MakeLoopDialogViewModel
            {
                NoLoop = true,
                InfiniteLoop = loopMode == LoopMode.Infinite,
                ConstrainedLoop = loopMode == LoopMode.Constrained,
                NumberOfLoops = numberOfLoops
            };
            makeLoopDialog.DataContext = makeLoopDialogViewModel;

            if (CurrentWindow == null)
            {
                throw new InvalidOperationException("Attempting to show a dialog but the current window that we use as the owner is null and that just won't do");
            }

            await makeLoopDialog.ShowDialog(CurrentWindow);

            if (makeLoopDialog.DialogResult != DialogResult.Confirm)
            {
                return (false, LoopMode.Unknown, null);
            }
            
            if (makeLoopDialogViewModel.NoLoop)
            {
                return (true, LoopMode.Unknown, null);
            }

            if (makeLoopDialogViewModel.InfiniteLoop)
            {
                return (true, LoopMode.Infinite, null);
            }

            if (makeLoopDialogViewModel.ConstrainedLoop)
            {
                return (true, LoopMode.Constrained, makeLoopDialogViewModel.NumberOfLoops);
            }

            return (true, LoopMode.Unknown, null);
        }

        public async Task ShowSaveRouteDialog(string? lastUsedFolder, RouteViewModel routeViewModel)
        {
            var saveRouteDialog = Resolve<SaveRouteDialog>();

            var viewModel = new SaveRouteDialogViewModel(
                this,
                routeViewModel,
                Resolve<RetrieveRepositoryNamesUseCase>(),
                Resolve<SaveRouteUseCase>(),
                Resolve<IUserPreferences>(),
                Resolve<IEnumerable<IRouteRepository>>());

            saveRouteDialog.DataContext = viewModel;
            
            if (CurrentWindow == null)
            {
                throw new InvalidOperationException("Attempting to show a dialog but the current window that we use as the owner is null and that just won't do");
            }

            await saveRouteDialog.ShowDialog(CurrentWindow);
        }

        public async Task<MessageBoxResult> ShowQuestionDialog(string title, string message)
        {
            return await MessageBox.ShowAsync(
                message,
                title,
                MessageBoxButton.YesNo,
                CurrentWindow!,
                MessageBoxIcon.Question);
        }

        public void ShowMainWindow(IApplicationLifetime applicationLifetime)
        {
            var desktopMainWindow = Resolve<MainWindow>();

            if (applicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: null } desktop)
            {
                desktop.MainWindow = desktopMainWindow;
            }

            Show(desktopMainWindow);
        }
    }
}
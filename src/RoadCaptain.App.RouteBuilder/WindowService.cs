﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.RouteBuilder.Views;
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.App.Shared.Dialogs.ViewModels;

namespace RoadCaptain.App.RouteBuilder
{
    public class WindowService : BaseWindowService, IWindowService
    {
        public WindowService(IComponentContext componentContext) : base(componentContext)
        {
        }

        public async Task<string?> ShowSaveFileDialog(string? previousLocation)
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
                    new() { Extensions = new List<string>{"*.json"}, Name = "RoadCaptain route file (.json)"},
                    new() { Extensions = new List<string>{"*.gpx"}, Name = "GPS Exchange Format (.gpx)"}
                },
                Title = "Save RoadCaptain route file",
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

        public async Task<MessageBoxResult> ShowSaveRouteDialog()
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
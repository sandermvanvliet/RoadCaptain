// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.App.Shared.Dialogs.ViewModels;
using RoadCaptain.App.Shared.Models;
using RoadCaptain.App.Shared.Views;

namespace RoadCaptain.App.Shared
{
    public abstract class BaseWindowService : IWindowService
    {
        private readonly IComponentContext _componentContext;
        private readonly MonitoringEvents _monitoringEvents;

        protected IClassicDesktopStyleApplicationLifetime? ApplicationLifetime { get; private set; }

        public BaseWindowService(IComponentContext componentContext, MonitoringEvents monitoringEvents)
        {
            _componentContext = componentContext;
            _monitoringEvents = monitoringEvents;
        }

        public Window? CurrentWindow { get; private set; }

        public void SetLifetime(IApplicationLifetime applicationLifetime)
        {
            ApplicationLifetime = applicationLifetime as IClassicDesktopStyleApplicationLifetime;
        }

        public void Shutdown(int exitCode)
        {
            ApplicationLifetime?.Shutdown(exitCode);
        }

        public async Task ShowAlreadyRunningDialog(string applicationName)
        {
            await MessageBox.ShowAsync(
                $"Only one instance of {applicationName} can be active",
                "Already running",
                MessageBoxButton.Ok,
                CurrentWindow!,
                MessageBoxIcon.Warning);
        }

        public virtual async Task<string?> ShowOpenFileDialog(string? previousLocation, IDictionary<string, string> filters)
        {
            var initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (!string.IsNullOrEmpty(previousLocation) && Directory.Exists(previousLocation))
            {
                initialDirectory = previousLocation;
            }
            
            if(CurrentWindow == null)
            {
                throw new InvalidOperationException("Attempting to show a dialog but the current window that we use as the owner is null and that just won't do");
            }

            var selectedFiles = await CurrentWindow.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Open RoadCaptain route file",
                    FileTypeFilter = filters.Select(f => new FilePickerFileType(f.Value) { Patterns = [f.Key] }).ToList().AsReadOnly(),
                    SuggestedStartLocation = await CurrentWindow.StorageProvider.TryGetFolderFromPathAsync(initialDirectory),
                    AllowMultiple = false
                });

            if (selectedFiles.Any())
            {
                return selectedFiles.First().Path.ToString();
            }

            return null;
        }

        public async Task ShowNewVersionDialog(Release release)
        {
            var window = Resolve<UpdateAvailableWindow>();

            window.DataContext = new UpdateAvailableViewModel(release);

            await ShowDialog(window);
        }

        public async Task ShowWhatIsNewDialog(Release release)
        {
            var window = Resolve<WhatIsNewWindow>();

            window.DataContext = new WhatIsNewViewModel(release);

            await ShowDialog(window);
        }

        public async Task<RouteModel?> ShowSelectRouteDialog()
        {
            var selectRouteWindow = Resolve<SelectRouteWindow>();

            selectRouteWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (await ShowDialog(selectRouteWindow) ?? false)
            {
                return selectRouteWindow.SelectedRoute;
            }

            return null;
        }

        public async Task ShowErrorDialog(string message)
        {
            await ShowErrorDialog(message, CurrentWindow ?? throw new ArgumentNullException(nameof(CurrentWindow)));
        }

        public virtual async Task ShowErrorDialog(string message, Window? owner)
        {
            await MessageBox.ShowAsync(
                message,
                "An error occurred",
                MessageBoxButton.Ok,
                owner,
                MessageBoxIcon.Error);
        }

        protected TType Resolve<TType>() where TType : notnull
        {
            try
            {
                return _componentContext.Resolve<TType>();
            }
            catch (Exception e)
            {
                _monitoringEvents.Error(e, "Unable to resolve window");
                throw;
            }
        }

        protected async Task<bool?> ShowDialog(Window window)
        {
            if (CurrentWindow == null)
            {
                throw new InvalidOperationException("Attempting to show a dialog but the current window that we use as the owner is null and that just won't do");
            }
            
            await window.ShowDialog(CurrentWindow);

            return true;
        }

        protected void Show(Window window)
        {
            CurrentWindow = window;

            window.Show();
        }

        protected void SwapWindows(Window window)
        {
            var toClose = CurrentWindow;

            Show(window);

            if (toClose != null)
            {
                Close(toClose);
            }

            CurrentWindow = window;
        }

        private void Close(Window window)
        {
            window.Close();
            CurrentWindow = null;
        }

        public async Task<TokenResponse?> ShowLogInDialog(Window owner)
        {
            var zwiftLoginWindow = Resolve<ZwiftLoginWindowBase>();

            zwiftLoginWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (await ShowDialog(zwiftLoginWindow) ?? false)
            {
                return zwiftLoginWindow.TokenResponse;
            }

            return null;
        }

        public virtual Window? GetCurrentWindow()
        {
            return CurrentWindow;
        }
    }
}

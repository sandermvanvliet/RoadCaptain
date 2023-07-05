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
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.App.Shared.Dialogs.ViewModels;

namespace RoadCaptain.App.Shared
{
    public class BaseWindowService : IWindowService
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

            var dialog = new OpenFileDialog
            {
                Directory = initialDirectory,
                AllowMultiple = false,
                Filters = filters
                    .Select(kv => new FileDialogFilter { Extensions = new List<string> { kv.Key }, Name = kv.Value })
                    .ToList(),
                Title = "Open RoadCaptain route file"
            };

            var selectedFiles = await dialog.ShowAsync(CurrentWindow ?? throw new ArgumentNullException(nameof(CurrentWindow)));

            if (selectedFiles != null && selectedFiles.Any())
            {
                return selectedFiles.First();
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
        
        public async Task ShowErrorDialog(string message)
        {
            await ShowErrorDialog(message, CurrentWindow ?? throw new ArgumentNullException(nameof(CurrentWindow)));
        }

        public virtual async Task ShowErrorDialog(string message, Window owner)
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
    }
}

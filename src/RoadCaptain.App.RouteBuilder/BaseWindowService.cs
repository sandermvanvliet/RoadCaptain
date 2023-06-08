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
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.App.Shared.Dialogs.ViewModels;

namespace RoadCaptain.App.RouteBuilder
{
    public abstract class BaseWindowService
    {
        private readonly IComponentContext _componentContext;
        private readonly MonitoringEvents _monitoringEvents;

        protected BaseWindowService(IComponentContext componentContext, MonitoringEvents monitoringEvents)
        {
            _componentContext = componentContext;
            _monitoringEvents = monitoringEvents;
        }

        protected Window? CurrentWindow { get; private set; }

        public async Task<string?> ShowOpenFileDialog(string? previousLocation)
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
                Filters = new List<FileDialogFilter>
                {
                    new() { Extensions = new List<string>{"json"}, Name = "RoadCaptain route file (.json)"}
                },
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

        public virtual async Task ShowErrorDialog(string message, Window owner)
        {
            await MessageBox.ShowAsync(
                message,
                "An error occurred",
                MessageBoxButton.Ok,
                owner ?? CurrentWindow,
                MessageBoxIcon.Error);
        }

        protected virtual TType Resolve<TType>() where TType : notnull
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

        protected virtual async Task<bool?> ShowDialog(Window window)
        {
            await window.ShowDialog(CurrentWindow);

            return true;
        }

        protected virtual void Show(Window window)
        {
            CurrentWindow = window;

            window.Show();
        }

        protected virtual void Close(Window window)
        {
            window.Close();
            CurrentWindow = null;
        }

        protected virtual bool Activate(Window window)
        {
            window.Activate();
            return true;
        }
    }
}

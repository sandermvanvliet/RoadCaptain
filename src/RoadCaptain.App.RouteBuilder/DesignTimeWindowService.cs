// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.App.Shared.Models;
using RouteViewModel = RoadCaptain.App.RouteBuilder.ViewModels.RouteViewModel;

namespace RoadCaptain.App.RouteBuilder
{
    internal class DesignTimeWindowService : IWindowService
    {
        public Task ShowErrorDialog(string message, Window? owner)
        {
            throw new System.NotImplementedException();
        }

        public void ShowMainWindow(IApplicationLifetime applicationLifetime)
        {
            throw new System.NotImplementedException();
        }

        public Task ShowAlreadyRunningDialog(string applicationName)
        {
            throw new System.NotImplementedException();
        }

        public Task<string?> ShowOpenFileDialog(string? previousLocation, IDictionary<string, string> filters)
        {
            throw new System.NotImplementedException();
        }

        public Task ShowNewVersionDialog(Release release)
        {
            throw new System.NotImplementedException();
        }

        public Task ShowWhatIsNewDialog(Release release)
        {
            throw new System.NotImplementedException();
        }

        public Task<string?> ShowSaveFileDialog(string? previousLocation, string? suggestedFileName = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> ShowDefaultSportSelectionDialog(SportType sport)
        {
            throw new System.NotImplementedException();
        }

        public Task<MessageBoxResult> ShowShouldSaveRouteDialog()
        {
            throw new System.NotImplementedException();
        }

        public Task<MessageBoxResult> ShowClearRouteDialog()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> ShowRouteLoopDialog()
        {
            throw new System.NotImplementedException();
        }

        public Task ShowSaveRouteDialog(string? lastUsedFolder, RouteViewModel routeViewModel)
        {
            throw new System.NotImplementedException();
        }

        public void Shutdown(int exitCode)
        {
            throw new System.NotImplementedException();
        }

        public Window? CurrentWindow { get; }

        public Task ShowAlreadyRunningDialog()
        {
            throw new System.NotImplementedException();
        }

        public void SetLifetime(IApplicationLifetime applicationLifetime)
        {
            throw new System.NotImplementedException();
        }

        public Task<TokenResponse?> ShowLogInDialog(Window owner)
        {
            throw new System.NotImplementedException();
        }

        public Window? GetCurrentWindow()
        {
            return null;
        }

        public Task ShowErrorDialog(string message)
        {
            return Task.CompletedTask;
        }
    }
}

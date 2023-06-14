// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared.Dialogs;

namespace RoadCaptain.App.RouteBuilder
{
    internal class DesignTimeWindowService : IWindowService
    {
        public Task<string?> ShowOpenFileDialog(string? previousLocation)
        {
            throw new System.NotImplementedException();
        }

        public Task ShowErrorDialog(string message, Window? owner)
        {
            throw new System.NotImplementedException();
        }

        public void ShowMainWindow(IApplicationLifetime applicationLifetime)
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

        public Task ShowAlreadyRunningDialog()
        {
            throw new System.NotImplementedException();
        }

        public void SetLifetime(IApplicationLifetime applicationLifetime)
        {
            throw new System.NotImplementedException();
        }
    }
}

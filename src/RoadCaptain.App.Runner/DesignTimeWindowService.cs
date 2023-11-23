// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.Runner
{
    public class DesignTimeWindowService : IWindowService
    {

        public void ShowInGameWindow(InGameNavigationWindowViewModel viewModel)
        {
            throw new System.NotImplementedException();
        }

        public Task<TokenResponse?> ShowLogInDialog(Window owner)
        {
            throw new System.NotImplementedException();
        }

        public Window? GetCurrentWindow()
        {
            throw new System.NotImplementedException();
        }

        public Task ShowErrorDialog(string message, Window? owner)
        {
            throw new System.NotImplementedException();
        }

        public Task ShowErrorDialog(string message)
        {
            throw new System.NotImplementedException();
        }

        public void ShowMainWindow()
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

        public void SetLifetime(IApplicationLifetime applicationLifetime)
        {
            throw new System.NotImplementedException();
        }

        public void Shutdown(int exitCode)
        {
            throw new System.NotImplementedException();
        }

        public Window? CurrentWindow { get; }

        public Task ShowWhatIsNewDialog(Release release)
        {
            throw new System.NotImplementedException();
        }

        public void ToggleElevationProfile(PlannedRoute? plannedRoute, bool? show)
        {
            throw new System.NotImplementedException();
        }

        public Task<RouteModel?> ShowSelectRouteDialog()
        {
            throw new System.NotImplementedException();
        }
    }
}

// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Runner.Views;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.Runner.Tests.Unit.ViewModels
{
    public class StubWindowService : IWindowService
    {
        public string? OpenFileDialogResult { get; set; }
        public TokenResponse? LogInDialogResult { get; set; }
        public int OpenFileDialogInvocations { get; private set; }
        public int LogInDialogInvocations { get; private set; }
        public int MainWindowInvocations { get; private set; }
        public int ErrorDialogInvocations { get; private set; }
        public int ShowSelectRouteDialogInvocations { get; private set; }
        public Dictionary<Type, object> Overrides { get; } = new();

        public List<Type> ClosedWindows { get; } = new();
        public List<Type> ShownWindows { get; } = new();
        public RouteModel? ShowSelectRouteDialogResult { get; set; }

        public Task ShowErrorDialog(string message, Window owner)
        {
            throw new NotImplementedException();
        }

        public Task ShowErrorDialog(string message)
        {
            ErrorDialogInvocations++;
            return Task.CompletedTask;
        }

        public void ShowMainWindow()
        {
            MainWindowInvocations++;
            ShownWindows.Add(typeof(Runner.Views.MainWindow));
            ClosedWindows.Add(typeof(InGameNavigationWindow));
        }

        public Task ShowAlreadyRunningDialog(string applicationName)
        {
            throw new NotImplementedException();
        }

        public Task<string?> ShowOpenFileDialog(string? previousLocation, IDictionary<string, string> filters)
        {
            throw new NotImplementedException();
        }

        public Task ShowNewVersionDialog(Release release)
        {
            throw new NotImplementedException();
        }

        public Task ShowAlreadyRunningDialog()
        {
            throw new NotImplementedException();
        }

        public void SetLifetime(IApplicationLifetime applicationLifetime)
        {
            throw new NotImplementedException();
        }

        public void Shutdown(int exitCode)
        {
            throw new NotImplementedException();
        }

        public Window? CurrentWindow { get; }

        public Task ShowWhatIsNewDialog(Release release)
        {
            throw new NotImplementedException();
        }

        public void ToggleElevationProfile(PlannedRoute? plannedRoute, bool? show)
        {
            throw new NotImplementedException();
        }

        public Task<RouteModel?> ShowSelectRouteDialog()
        {
            ShowSelectRouteDialogInvocations++;
            return Task.FromResult<RouteModel?>(ShowSelectRouteDialogResult);
        }

        public Task<string?> ShowOpenFileDialog(string? previousLocation)
        {
            OpenFileDialogInvocations++;
            return Task.FromResult(OpenFileDialogResult);
        }

        public void ShowInGameWindow(InGameNavigationWindowViewModel viewModel)
        {
            ShownWindows.Add(typeof(InGameNavigationWindow));
        }

        public Task<TokenResponse?> ShowLogInDialog(Window owner)
        {
            LogInDialogInvocations++;
            return Task.FromResult(LogInDialogResult);
        }
    }
}

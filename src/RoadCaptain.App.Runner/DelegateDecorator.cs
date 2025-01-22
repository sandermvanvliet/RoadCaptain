// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.Runner
{
    public class DelegateDecorator : IWindowService
    {
        private readonly IWindowService _decorated;
        private readonly Dispatcher _dispatcher;

        public DelegateDecorator(IWindowService decorated, Dispatcher dispatcher)
        {
            _decorated = decorated;
            _dispatcher = dispatcher;
        }

        public async Task ShowAlreadyRunningDialog(string applicationName)
        {
            await InvokeIfNeededAsync(() => _decorated.ShowAlreadyRunningDialog(applicationName));
        }

        public async Task<string?> ShowOpenFileDialog(string? previousLocation, IDictionary<string, string> filters)
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowOpenFileDialog(previousLocation, filters));
        }

        public void ShowInGameWindow(InGameNavigationWindowViewModel viewModel)
        {
            InvokeIfNeeded(() => _decorated.ShowInGameWindow(viewModel));
        }

        public async Task<TokenResponse?> ShowLogInDialog(Window owner)
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowLogInDialog(owner));
        }

        public Window? GetCurrentWindow()
        {
            return InvokeIfNeeded(() => _decorated.GetCurrentWindow());
        }

        public async Task ShowErrorDialog(string message, Window? owner)
        {
            await InvokeIfNeededAsync(() => _decorated.ShowErrorDialog(message, owner));
        }

        public async Task ShowErrorDialog(string message)
        {
            await InvokeIfNeededAsync(() => _decorated.ShowErrorDialog(message));
        }

        public void ShowMainWindow()
        {
            InvokeIfNeeded(() => _decorated.ShowMainWindow());
        }

        public async Task ShowNewVersionDialog(Release release)
        {
            await InvokeIfNeededAsync(() => _decorated.ShowNewVersionDialog(release));
        }

        public void SetLifetime(IApplicationLifetime applicationLifetime)
        {
            _decorated.SetLifetime(applicationLifetime);
        }

        public void Shutdown(int exitCode)
        {
            InvokeIfNeeded(() => _decorated.Shutdown(exitCode));
        }

        public Window? CurrentWindow => _decorated.CurrentWindow;

        public async Task ShowWhatIsNewDialog(Release release)
        {
            await InvokeIfNeededAsync(() => _decorated.ShowWhatIsNewDialog(release));
        }

        public void ToggleElevationProfile(PlannedRoute? plannedRoute, bool? show)
        {
            InvokeIfNeeded(() => _decorated.ToggleElevationProfile(plannedRoute, show));
        }

        public Task<RouteModel?> ShowSelectRouteDialog()
        {
            return InvokeIfNeededAsync(() => _decorated.ShowSelectRouteDialog());
        }

        private async Task<TResult> InvokeIfNeededAsync<TResult>(Func<Task<TResult>> action)
        {
            if (!_dispatcher.CheckAccess())
            {
                return await _dispatcher.InvokeAsync(action);
            }

            return await action();
        }

        private async Task InvokeIfNeededAsync(Func<Task> action)
        {
            if (!_dispatcher.CheckAccess())
            {
                await _dispatcher.InvokeAsync(action);
            }

            await action();
        }

        private void InvokeIfNeeded(Action action)
        {
            if (!_dispatcher.CheckAccess())
            {
                _dispatcher.InvokeAsync(action).GetAwaiter().GetResult();
            }
            else
            {
                action();
            }
        }

        private TValue InvokeIfNeeded<TValue>(Func<TValue> action)
        {
            if (!_dispatcher.CheckAccess())
            {
                return _dispatcher.InvokeAsync(action).GetAwaiter().GetResult();
            }
            else
            {
                return action();
            }
        }
    }
}

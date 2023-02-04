// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
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

        public async Task<string?> ShowOpenFileDialog(string? previousLocation)
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowOpenFileDialog(previousLocation));
        }

        public void ShowInGameWindow(InGameNavigationWindowViewModel viewModel)
        {
            InvokeIfNeeded(() => _decorated.ShowInGameWindow(viewModel));
        }

        public async Task<TokenResponse?> ShowLogInDialog(Window owner)
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowLogInDialog(owner));
        }

        public async Task ShowErrorDialog(string message, Window owner)
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

        public async Task ShowAlreadyRunningDialog()
        {
            await InvokeIfNeededAsync(() => _decorated.ShowAlreadyRunningDialog());
        }

        public void SetLifetime(IApplicationLifetime applicationLifetime)
        {
            _decorated.SetLifetime(applicationLifetime);
        }

        public void Shutdown(int exitCode)
        {
            InvokeIfNeeded(() => _decorated.Shutdown(exitCode));
        }

        public async Task ShowWhatIsNewDialog(Release release)
        {
            await InvokeIfNeededAsync(() => _decorated.ShowWhatIsNewDialog(release));
        }

        public void ToggleElevationPlot(PlannedRoute? plannedRoute, bool? show)
        {
            InvokeIfNeeded(() => _decorated.ToggleElevationPlot(plannedRoute, show));
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
    }
}

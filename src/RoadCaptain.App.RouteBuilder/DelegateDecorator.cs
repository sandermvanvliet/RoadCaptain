// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.App.Shared.Models;
using RouteViewModel = RoadCaptain.App.RouteBuilder.ViewModels.RouteViewModel;

namespace RoadCaptain.App.RouteBuilder
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

        public async Task<string?> ShowOpenFileDialog(string? previousLocation, IDictionary<string, string> filters)
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowOpenFileDialog(previousLocation, filters));
        }

        public async Task ShowErrorDialog(string message)
        {
            await InvokeIfNeededAsync(() => _decorated.ShowErrorDialog(message));
        }

        public async Task ShowErrorDialog(string message, Window? owner)
        {
            await InvokeIfNeededAsync(() => _decorated.ShowErrorDialog(message, owner));
        }

        public void ShowMainWindow(IApplicationLifetime applicationLifetime)
        {
            InvokeIfNeeded(() => _decorated.ShowMainWindow(applicationLifetime));
        }

        public async Task ShowNewVersionDialog(Release release)
        {
            await InvokeIfNeededAsync(() => _decorated.ShowNewVersionDialog(release));
        }

        public async Task ShowWhatIsNewDialog(Release release)
        {
            await InvokeIfNeededAsync(() => _decorated.ShowWhatIsNewDialog(release));
        }

        public async Task<RouteModel?> ShowSelectRouteDialog()
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowSelectRouteDialog());
        }

        public async Task<string?> ShowSaveFileDialog(string? previousLocation, string? suggestedFileName = null)
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowSaveFileDialog(previousLocation, suggestedFileName));
        }

        public async Task<(PlannedRoute? PlannedRoute, string? RouteFilePath)> ShowOpenRouteDialog()
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowOpenRouteDialog());
        }

        public async Task<bool> ShowDefaultSportSelectionDialog(SportType sport)
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowDefaultSportSelectionDialog(sport));
        }

        public async Task<MessageBoxResult> ShowShouldSaveRouteDialog()
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowShouldSaveRouteDialog());
        }

        public async Task<MessageBoxResult> ShowClearRouteDialog()
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowClearRouteDialog());
        }

        public async Task<(LoopMode Mode, int? NumberOfLoops)> ShowRouteLoopDialog()
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowRouteLoopDialog());
        }

        public async Task ShowSaveRouteDialog(string? lastUsedFolder, RouteViewModel routeViewModel)
        {
            await InvokeIfNeededAsync(() => _decorated.ShowSaveRouteDialog(lastUsedFolder, routeViewModel));
        }

        public void Shutdown(int exitCode)
        {
            InvokeIfNeeded(() => _decorated.Shutdown(exitCode));
        }

        public Window? CurrentWindow => _decorated.CurrentWindow;

        public async Task ShowAlreadyRunningDialog(string applicationName)
        {
            await InvokeIfNeededAsync(() => _decorated.ShowAlreadyRunningDialog(applicationName));
        }

        public void SetLifetime(IApplicationLifetime applicationLifetime)
        {
            InvokeIfNeeded(() => _decorated.SetLifetime(applicationLifetime));
        }

        public Task<TokenResponse?> ShowLogInDialog(Window owner)
        {
            return InvokeIfNeededAsync(() => _decorated.ShowLogInDialog(owner));
        }

        public Window? GetCurrentWindow()
        {
            return _decorated.GetCurrentWindow();
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

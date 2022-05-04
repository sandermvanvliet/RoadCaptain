using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using RoadCaptain.App.Shared.Dialogs;

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

        public async Task<string?> ShowOpenFileDialog(string? previousLocation)
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowOpenFileDialog(previousLocation));
        }

        public void ShowErrorDialog(string message, Window owner)
        {
            InvokeIfNeeded(() => _decorated.ShowErrorDialog(message, owner));
        }

        public void ShowMainWindow(IApplicationLifetime applicationLifetime)
        {
            InvokeIfNeeded(() => _decorated.ShowMainWindow(applicationLifetime));
        }

        public void ShowNewVersionDialog(Release release)
        {
            InvokeIfNeeded(() => _decorated.ShowNewVersionDialog(release));
        }

        public async Task<string?> ShowSaveFileDialog(string? previousLocation)
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowSaveFileDialog(previousLocation));
        }

        public async Task<bool> ShowDefaultSportSelectionDialog(SportType sport)
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowDefaultSportSelectionDialog(sport));
        }

        public async Task<MessageBoxResult> ShowSaveRouteDialog()
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowSaveRouteDialog());
        }

        public async Task<MessageBoxResult> ShowClearRouteDialog()
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowClearRouteDialog());
        }

        private async Task<TResult> InvokeIfNeededAsync<TResult>(Func<Task<TResult>> action)
        {
            if (!_dispatcher.CheckAccess())
            {
                return await _dispatcher.InvokeAsync(action);
            }

            return await action();
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
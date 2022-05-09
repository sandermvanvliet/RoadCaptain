using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.App.Runner.ViewModels;

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

        public async Task<string?> ShowOpenFileDialog(string previousLocation)
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowOpenFileDialog(previousLocation));
        }

        public void ShowInGameWindow(InGameNavigationWindowViewModel viewModel)
        {
            InvokeIfNeeded(() => _decorated.ShowInGameWindow(viewModel));
        }

        public async Task<TokenResponse> ShowLogInDialog(Window owner)
        {
            return await InvokeIfNeededAsync(() => _decorated.ShowLogInDialog(owner));
        }

        public async Task ShowErrorDialog(string message, Window owner = null)
        {
            await InvokeIfNeededAsync(() => _decorated.ShowErrorDialog(message, owner));
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
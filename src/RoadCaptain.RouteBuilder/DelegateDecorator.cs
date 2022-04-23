using System;
using System.Windows;
using System.Windows.Threading;

namespace RoadCaptain.RouteBuilder
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

        public string ShowOpenFileDialog(string previousLocation)
        {
            return InvokeIfNeeded(() => _decorated.ShowOpenFileDialog(previousLocation));
        }

        public void ShowErrorDialog(string message, Window owner = null)
        {
            InvokeIfNeeded(() => _decorated.ShowErrorDialog(message, owner));
        }

        public void ShowMainWindow()
        {
            InvokeIfNeeded(() => _decorated.ShowMainWindow());
        }

        public void ShowNewVersionDialog(Release release)
        {
            InvokeIfNeeded(() => _decorated.ShowNewVersionDialog(release));
        }

        public string ShowSaveFileDialog(string previousLocation)
        {
            return InvokeIfNeeded(() => _decorated.ShowSaveFileDialog(previousLocation));
        }

        public bool ShowDefaultSportSelectionDialog(SportType sport)
        {
            return InvokeIfNeeded(() => _decorated.ShowDefaultSportSelectionDialog(sport));
        }

        public MessageBoxResult ShowSaveRouteDialog()
        {
            return InvokeIfNeeded(() => _decorated.ShowSaveRouteDialog());
        }

        private TResult InvokeIfNeeded<TResult>(Func<TResult> action)
        {
            if (!_dispatcher.CheckAccess())
            {
                return _dispatcher.Invoke(action);
            }

            return action();
        }

        private void InvokeIfNeeded(Action action)
        {
            if (!_dispatcher.CheckAccess())
            {
                _dispatcher.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}
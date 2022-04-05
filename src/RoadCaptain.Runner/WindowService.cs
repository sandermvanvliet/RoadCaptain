using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using Microsoft.Win32;
using RoadCaptain.Runner.Models;
using RoadCaptain.Runner.ViewModels;

namespace RoadCaptain.Runner
{
    public class WindowService : IWindowService
    {
        private readonly IComponentContext _componentContext;
        private readonly Dispatcher _dispatcher;
        private Window _currentWindow;

        public WindowService(IComponentContext componentContext, Dispatcher dispatcher)
        {
            _componentContext = componentContext;
            _dispatcher = dispatcher;
        }

        public string ShowOpenFileDialog()
        {
            var dialog = new OpenFileDialog
            {
                RestoreDirectory = true,
                AddExtension = true,
                DefaultExt = ".json",
                Filter = "JSON files (.json)|*.json",
                Multiselect = false
            };

            var result = dialog.ShowDialog() ?? false;

            return result
                ? dialog.FileName
                : null;
        }

        public void ShowMainWindow()
        {
            _dispatcher.Invoke(() =>
            {
                if (_currentWindow is MainWindow)
                {
                    _currentWindow.Activate();
                }
                else
                {
                    var window = _componentContext.Resolve<MainWindow>();

                    if (_currentWindow != null)
                    {
                        _currentWindow.Close();
                        _currentWindow = null;
                    }

                    _currentWindow = window;

                    window.Show();
                }
            });
        }

        public void ShowInGameWindow(Window owner, InGameNavigationWindowViewModel viewModel)
        {
            _dispatcher.Invoke(() =>
            {
                var inGameWindow = _componentContext.Resolve<InGameNavigationWindow>();

                inGameWindow.DataContext = viewModel;

                inGameWindow.Show();

                owner.Close();
            });
        }

        public TokenResponse ShowLogInDialog(Window owner)
        {
            if (_dispatcher.Thread != Thread.CurrentThread)
            {
                return _dispatcher.Invoke(() => InnerShowLogInDialog(owner));
            }

            return InnerShowLogInDialog(owner);
        }

        private TokenResponse InnerShowLogInDialog(Window owner)
        {
            var zwiftLoginWindow = _componentContext.Resolve<ZwiftLoginWindow>();

            zwiftLoginWindow.Owner = owner;
            zwiftLoginWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (zwiftLoginWindow.ShowDialog() ?? false)
            {
                return zwiftLoginWindow.TokenResponse;
            }

            return null;
        }

        public void ShowErrorDialog(string message, Window owner)
        {
            _dispatcher.Invoke(() =>
            {
                if (owner != null)
                {
                    MessageBox.Show(
                        owner,
                        message,
                        "An error occurred",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                else if(_currentWindow != null)
                {
                    MessageBox.Show(
                        _currentWindow,
                        message,
                        "An error occurred",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(
                        message,
                        "An error occurred",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            });
        }
    }

    public interface IWindowService
    {
        string ShowOpenFileDialog();
        void ShowInGameWindow(Window owner, InGameNavigationWindowViewModel viewModel);
        TokenResponse ShowLogInDialog(Window owner);
        void ShowErrorDialog(string message, Window owner = null);
        void ShowMainWindow();
    }
}
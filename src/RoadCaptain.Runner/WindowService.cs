using System.Windows;
using Autofac;
using Microsoft.Win32;
using RoadCaptain.Runner.Models;
using RoadCaptain.Runner.ViewModels;

namespace RoadCaptain.Runner
{
    public class WindowService : IWindowService
    {
        private readonly IComponentContext _componentContext;
        private Window _currentWindow;

        public WindowService(IComponentContext componentContext)
        {
            _componentContext = componentContext;
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
        }

        public void ShowInGameWindow(Window owner, InGameNavigationWindowViewModel viewModel)
        {
                var inGameWindow = _componentContext.Resolve<InGameNavigationWindow>();

                inGameWindow.DataContext = viewModel;

                inGameWindow.Show();

                owner.Close();
        }

        public TokenResponse ShowLogInDialog(Window owner)
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
                if (owner != null)
                {
                    MessageBox.Show(
                        owner,
                        message,
                        "An error occurred",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                else if (_currentWindow != null)
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
        }
    }
}
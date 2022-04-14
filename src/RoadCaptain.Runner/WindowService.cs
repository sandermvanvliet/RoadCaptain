using System;
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
                Multiselect = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            var result = ShowDialog(dialog).GetValueOrDefault();

            return result
                ? dialog.FileName
                : null;
        }

        public void ShowMainWindow()
        {
            if (_currentWindow is MainWindow)
            {
                Activate(_currentWindow);
            }
            else
            {
                var window = Resolve<MainWindow>();

                if (_currentWindow != null)
                {
                    Close(_currentWindow);
                    _currentWindow = null;
                }

                _currentWindow = window;

                Show(window);
            }
        }

        public void ShowNewVersionDialog(Release release)
        {
            var window = Resolve<UpdateAvailableWindow>();

            window.DataContext = new UpdateAvailableViewModel(release);
            window.Owner = _currentWindow;

            ShowDialog(window);
        }

        public void ShowInGameWindow(InGameNavigationWindowViewModel viewModel)
        {
            var inGameWindow = Resolve<InGameNavigationWindow>();

            inGameWindow.DataContext = viewModel;

            Show(inGameWindow);

            if (_currentWindow != null)
            {
                Close(_currentWindow);
            }

            _currentWindow = inGameWindow;
        }

        public TokenResponse ShowLogInDialog(Window owner)
        {
            var zwiftLoginWindow = Resolve<ZwiftLoginWindow>();

            zwiftLoginWindow.Owner = owner;
            zwiftLoginWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (ShowDialog(zwiftLoginWindow) ?? false)
            {
                return zwiftLoginWindow.TokenResponse;
            }

            return null;
        }

        public virtual void ShowErrorDialog(string message, Window owner)
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

        protected virtual TType Resolve<TType>()
        {
            return _componentContext.Resolve<TType>();
        }

        protected virtual bool? ShowDialog(Window window)
        {
            return window.ShowDialog();
        }

        protected virtual void Show(Window window)
        {
            window.Show();
        }

        protected virtual void Close(Window window)
        {
            window.Close();
        }

        protected virtual bool Activate(Window window)
        {
            return window.Activate();
        }

        protected virtual bool? ShowDialog(CommonDialog dialog)
        {
            return dialog.ShowDialog(_currentWindow);
        }
    }
}
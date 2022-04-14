using System;
using System.Windows;
using Autofac;
using Microsoft.Win32;
using RoadCaptain.UserInterface.Shared.ViewModels;

namespace RoadCaptain.UserInterface.Shared
{
    public abstract class BaseWindowService
    {
        private readonly IComponentContext _componentContext;

        protected BaseWindowService(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        protected Window? CurrentWindow { get; private set; }

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

        public void ShowNewVersionDialog(Release release)
        {
            var window = Resolve<UpdateAvailableWindow>();

            window.DataContext = new UpdateAvailableViewModel(release);
            window.Owner = CurrentWindow;

            ShowDialog(window);
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
            else if (CurrentWindow != null)
            {
                MessageBox.Show(
                    CurrentWindow,
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
            CurrentWindow = window;
            window.Show();
        }

        protected virtual void Close(Window window)
        {
            window.Close();
            CurrentWindow = null;
        }

        protected virtual bool Activate(Window window)
        {
            return window.Activate();
        }

        protected virtual bool? ShowDialog(CommonDialog dialog)
        {
            return dialog.ShowDialog(CurrentWindow);
        }
    }
}
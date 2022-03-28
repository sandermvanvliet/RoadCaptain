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

        public void ShowErrorDialog(string message)
        {
            MessageBox.Show(
                message,
                "An error occurred",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public interface IWindowService
    {
        string ShowOpenFileDialog();
        void ShowInGameWindow(Window owner, InGameNavigationWindowViewModel viewModel);
        TokenResponse ShowLogInDialog(Window owner);
        void ShowErrorDialog(string message);
    }
}
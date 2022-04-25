using System.Windows;
using Autofac;
using RoadCaptain.Runner.Models;
using RoadCaptain.Runner.ViewModels;
using RoadCaptain.UserInterface.Shared;

namespace RoadCaptain.Runner
{
    public class WindowService : BaseWindowService, IWindowService
    {
        public WindowService(IComponentContext componentContext) : base(componentContext)
        {
        }

        public void ShowMainWindow()
        {
            if (CurrentWindow is MainWindow)
            {
                Activate(CurrentWindow);
            }
            else
            {
                var window = Resolve<MainWindow>();

                if (CurrentWindow != null)
                {
                    Close(CurrentWindow);
                }
                
                Show(window);
            }
        }

        public void ShowAlreadyRunningDialog()
        {
            MessageBox.Show(
                "Only one instance of RoadCaptain Runner can be active",
                "Already running",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        public void ShowInGameWindow(InGameNavigationWindowViewModel viewModel)
        {
            var inGameWindow = Resolve<InGameNavigationWindow>();

            inGameWindow.DataContext = viewModel;

            if (CurrentWindow != null)
            {
                Close(CurrentWindow);
            }

            Show(inGameWindow);
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
    }
}
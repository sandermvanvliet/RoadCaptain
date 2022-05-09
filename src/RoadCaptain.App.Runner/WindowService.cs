using Autofac;
using Avalonia.Controls;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Runner.Views;
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.App.Shared.Dialogs.ViewModels;

namespace RoadCaptain.App.Runner
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
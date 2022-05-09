using System.Threading.Tasks;
using Autofac;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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

        public void ShowMainWindow(IApplicationLifetime applicationLifetime)
        {
            var desktopMainWindow = Resolve<MainWindow>();

            if (applicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow == null)
            {
                desktop.MainWindow = desktopMainWindow;
            }

            base.Show(desktopMainWindow);
        }

        public async Task ShowAlreadyRunningDialog()
        {
            await MessageBox.ShowAsync(
                "Only one instance of RoadCaptain Runner can be active",
                "Already running",
                MessageBoxButton.Ok,
                CurrentWindow,
                MessageBoxIcon.Warning);
        }

        public void ShowInGameWindow(InGameNavigationWindowViewModel viewModel)
        {
            //var inGameWindow = Resolve<InGameNavigationWindow>();

            //inGameWindow.DataContext = viewModel;

            //if (CurrentWindow != null)
            //{
            //    Close(CurrentWindow);
            //}

            //Show(inGameWindow);
        }

        public async Task<TokenResponse?> ShowLogInDialog(Window owner)
        {
            //var zwiftLoginWindow = Resolve<ZwiftLoginWindow>();

            //zwiftLoginWindow.Owner = owner;
            //zwiftLoginWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            //if (ShowDialog(zwiftLoginWindow) ?? false)
            //{
            //    return zwiftLoginWindow.TokenResponse;
            //}

            return null;
        }
    }
}
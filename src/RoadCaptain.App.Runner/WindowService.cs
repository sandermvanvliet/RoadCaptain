using System.Threading.Tasks;
using Autofac;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Runner.Views;
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.App.Shared.Dialogs.ViewModels;
using RoadCaptain.App.Shared.Models;
using RoadCaptain.App.Shared.Views;

namespace RoadCaptain.App.Runner
{
    public class WindowService : BaseWindowService, IWindowService
    {
        private IClassicDesktopStyleApplicationLifetime _applicationLifetime;

        public WindowService(IComponentContext componentContext, MonitoringEvents monitoringEvents) : base(componentContext, monitoringEvents)
        {
        }

        public void SetLifetime(IApplicationLifetime applicationLifetime)
        {
            _applicationLifetime = applicationLifetime as IClassicDesktopStyleApplicationLifetime;
        }

        public void Shutdown(int exitCode)
        {
            _applicationLifetime.Shutdown(exitCode);
        }

        public void ShowMainWindow()
        {
            if (CurrentWindow is MainWindow)
            {
                CurrentWindow.Activate();
                return;
            }

            var mainWindow = Resolve<MainWindow>();
            
            _applicationLifetime.MainWindow = mainWindow;
            
            SwapWindows(mainWindow);
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
            if (CurrentWindow is InGameNavigationWindow)
            {
                //CurrentWindow.Activate();
                return;
            }

            var inGameWindow = Resolve<InGameNavigationWindow>();

            inGameWindow.DataContext = viewModel;

            _applicationLifetime.MainWindow = inGameWindow;

            SwapWindows(inGameWindow);
        }

        public virtual async Task<TokenResponse?> ShowLogInDialog(Window owner)
        {
            var zwiftLoginWindow = Resolve<ZwiftLoginWindowBase>();

            zwiftLoginWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (await ShowDialog(zwiftLoginWindow) ?? false)
            {
                return zwiftLoginWindow.TokenResponse;
            }

            return null;
        }
    }
}
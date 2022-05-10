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
            var mainWindow = Resolve<MainWindow>();
            
            if (CurrentWindow != null)
            {
                _applicationLifetime.MainWindow = mainWindow;
                Close(CurrentWindow);
            }

            base.Show(mainWindow);
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
            var inGameWindow = Resolve<InGameNavigationWindow>();

            inGameWindow.DataContext = viewModel;

            if (CurrentWindow != null)
            {
                _applicationLifetime.MainWindow = inGameWindow;
                Close(CurrentWindow);
            }

            Show(inGameWindow);
        }

        public async Task<TokenResponse?> ShowLogInDialog(Window owner)
        {
            var zwiftLoginWindow = Resolve<ZwiftLoginWindow>();
            
            zwiftLoginWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (await ShowDialog(zwiftLoginWindow) ?? false)
            {
                return zwiftLoginWindow.TokenResponse;
            }

            return null;
        }
    }
}
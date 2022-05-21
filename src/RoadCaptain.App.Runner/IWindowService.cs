using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.App.Runner.ViewModels;

namespace RoadCaptain.App.Runner
{
    public interface IWindowService
    {
        Task<string?> ShowOpenFileDialog(string? previousLocation);
        void ShowInGameWindow(InGameNavigationWindowViewModel viewModel);
        Task<TokenResponse?> ShowLogInDialog(Window owner);
        Task ShowErrorDialog(string message, Window owner);
        Task ShowErrorDialog(string message);
        void ShowMainWindow();
        Task ShowNewVersionDialog(Release release);
        Task ShowAlreadyRunningDialog();
        void SetLifetime(IApplicationLifetime applicationLifetime);
        void Shutdown(int exitCode);
    }
}
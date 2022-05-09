using System.Threading.Tasks;
using Avalonia.Controls;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.App.Runner.ViewModels;

namespace RoadCaptain.App.Runner
{
    public interface IWindowService
    {
        Task<string?> ShowOpenFileDialog(string previousLocation);
        void ShowInGameWindow(InGameNavigationWindowViewModel viewModel);
        Task<TokenResponse> ShowLogInDialog(Window owner);
        Task ShowErrorDialog(string message, Window owner = null);
        void ShowMainWindow();
        Task ShowNewVersionDialog(Release release);
        Task ShowAlreadyRunningDialog();
    }
}
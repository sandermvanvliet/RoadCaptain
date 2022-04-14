using System.Windows;
using RoadCaptain.Runner.Models;
using RoadCaptain.Runner.ViewModels;

namespace RoadCaptain.Runner
{
    public interface IWindowService
    {
        string ShowOpenFileDialog();
        void ShowInGameWindow(InGameNavigationWindowViewModel viewModel);
        TokenResponse ShowLogInDialog(Window owner);
        void ShowErrorDialog(string message, Window owner = null);
        void ShowMainWindow();
        void ShowNewVersionDialog(Release release);
    }
}
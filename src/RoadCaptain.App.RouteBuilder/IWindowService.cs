using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace RoadCaptain.App.RouteBuilder
{
    public interface IWindowService
    {
        string? ShowOpenFileDialog(string? previousLocation);
        void ShowErrorDialog(string message, Window owner = null);
        void ShowMainWindow(IApplicationLifetime applicationLifetime);
        void ShowNewVersionDialog(Release release);
        string? ShowSaveFileDialog(string? previousLocation);
        bool ShowDefaultSportSelectionDialog(SportType sport);
        MessageBoxResult ShowSaveRouteDialog();
        MessageBoxResult ShowClearRouteDialog();
    }

    public enum MessageBoxResult
    {
        Cancel,
        Yes,
        No
    }
}
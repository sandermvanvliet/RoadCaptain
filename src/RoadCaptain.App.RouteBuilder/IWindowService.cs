using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.Shared.Dialogs;

namespace RoadCaptain.App.RouteBuilder
{
    public interface IWindowService
    {
        string? ShowOpenFileDialog(string? previousLocation);
        void ShowErrorDialog(string message, Window owner);
        void ShowMainWindow(IApplicationLifetime applicationLifetime);
        void ShowNewVersionDialog(Release release);
        string? ShowSaveFileDialog(string? previousLocation);
        Task<bool> ShowDefaultSportSelectionDialog(SportType sport);
        MessageBoxResult ShowSaveRouteDialog();
        MessageBoxResult ShowClearRouteDialog();
    }
}
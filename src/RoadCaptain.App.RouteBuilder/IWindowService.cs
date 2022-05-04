using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.Shared.Dialogs;

namespace RoadCaptain.App.RouteBuilder
{
    public interface IWindowService
    {
        Task<string?> ShowOpenFileDialog(string? previousLocation);
        void ShowErrorDialog(string message, Window owner);
        void ShowMainWindow(IApplicationLifetime applicationLifetime);
        void ShowNewVersionDialog(Release release);
        Task<string?> ShowSaveFileDialog(string? previousLocation);
        Task<bool> ShowDefaultSportSelectionDialog(SportType sport);
        Task<MessageBoxResult> ShowSaveRouteDialog();
        Task<MessageBoxResult> ShowClearRouteDialog();
    }
}
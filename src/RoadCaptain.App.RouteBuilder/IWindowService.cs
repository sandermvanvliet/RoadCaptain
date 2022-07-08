using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared.Dialogs;

namespace RoadCaptain.App.RouteBuilder
{
    public interface IWindowService
    {
        Task<string?> ShowOpenFileDialog(string? previousLocation);
        Task ShowErrorDialog(string message, Window owner);
        void ShowMainWindow(IApplicationLifetime applicationLifetime);
        Task ShowNewVersionDialog(Release release);
        Task ShowWhatIsNewDialog(Release release);
        Task<string?> ShowSaveFileDialog(string? previousLocation, string? suggestedFileName = null);
        Task<bool> ShowDefaultSportSelectionDialog(SportType sport);
        Task<MessageBoxResult> ShowShouldSaveRouteDialog();
        Task<MessageBoxResult> ShowClearRouteDialog();
        Task<bool> ShowRouteLoopDialog();
        Task<string?> ShowSaveRouteDialog(string? lastUsedFolder, RouteViewModel routeViewModel);
    }
}
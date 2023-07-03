// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.RouteBuilder
{
    public interface IWindowService
    {
        Task<string?> ShowOpenFileDialog(string? previousLocation);
        Task ShowErrorDialog(string message, Window? owner);
        void ShowMainWindow(IApplicationLifetime applicationLifetime);
        Task ShowNewVersionDialog(Release release);
        Task ShowWhatIsNewDialog(Release release);
        Task<string?> ShowSaveFileDialog(string? previousLocation, string? suggestedFileName = null);
        Task<bool> ShowDefaultSportSelectionDialog(SportType sport);
        Task<MessageBoxResult> ShowShouldSaveRouteDialog();
        Task<MessageBoxResult> ShowClearRouteDialog();
        Task<bool> ShowRouteLoopDialog();
        Task ShowSaveRouteDialog(string? lastUsedFolder, RouteViewModel routeViewModel);
        void Shutdown(int exitCode);
        Task ShowAlreadyRunningDialog();
        void SetLifetime(IApplicationLifetime applicationLifetime);
        Task<TokenResponse?> ShowLogInDialog(Window owner);
        Window? GetCurrentWindow();
    }
}
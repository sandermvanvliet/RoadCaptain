// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.Shared.Dialogs;
using RoadCaptain.App.Shared.Models;
using RouteViewModel = RoadCaptain.App.RouteBuilder.ViewModels.RouteViewModel;

namespace RoadCaptain.App.RouteBuilder
{
    public interface IWindowService : RoadCaptain.App.Shared.IWindowService
    {
        void ShowMainWindow(IApplicationLifetime applicationLifetime);
        Task<bool> ShowDefaultSportSelectionDialog(SportType sport);
        Task<MessageBoxResult> ShowShouldSaveRouteDialog();
        Task<MessageBoxResult> ShowClearRouteDialog();
        Task<bool> ShowRouteLoopDialog();
        Task ShowSaveRouteDialog(string? lastUsedFolder, RouteViewModel routeViewModel);
        Task<TokenResponse?> ShowLogInDialog(Window owner);
        Window? GetCurrentWindow();
        Task<string?> ShowSaveFileDialog(string? previousLocation, string? suggestedFileName = null);
    }
}
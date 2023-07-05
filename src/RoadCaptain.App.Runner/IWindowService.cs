// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Threading.Tasks;
using Avalonia.Controls;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.Runner
{
    public interface IWindowService : RoadCaptain.App.Shared.IWindowService
    {
        void ShowInGameWindow(InGameNavigationWindowViewModel viewModel);
        Task<TokenResponse?> ShowLogInDialog(Window owner);
        void ShowMainWindow();
        void ToggleElevationPlot(PlannedRoute? plannedRoute, bool? show);
    }
}

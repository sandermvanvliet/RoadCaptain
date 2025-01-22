// Copyright (c) 2025 Sander van Vliet
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
        void ShowMainWindow();
        void ToggleElevationProfile(PlannedRoute? plannedRoute, bool? show);
    }
}

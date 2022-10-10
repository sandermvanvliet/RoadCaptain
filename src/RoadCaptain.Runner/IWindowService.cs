// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Windows;
using RoadCaptain.Runner.Models;
using RoadCaptain.Runner.ViewModels;

namespace RoadCaptain.Runner
{
    public interface IWindowService
    {
        string ShowOpenFileDialog(string previousLocation);
        void ShowInGameWindow(InGameNavigationWindowViewModel viewModel);
        TokenResponse ShowLogInDialog(Window owner);
        void ShowErrorDialog(string message, Window owner = null);
        void ShowMainWindow();
        void ShowNewVersionDialog(Release release);
        void ShowAlreadyRunningDialog();
    }
}

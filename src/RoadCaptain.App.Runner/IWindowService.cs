// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.Runner
{
    public interface IWindowService
    {
        Task<string?> ShowOpenFileDialog(string? previousLocation);
        void ShowInGameWindow(InGameNavigationWindowViewModel viewModel);
        Task<TokenResponse?> ShowLogInDialog(Window owner);
        Task ShowErrorDialog(string message, Window owner);
        Task ShowErrorDialog(string message);
        void ShowMainWindow();
        Task ShowNewVersionDialog(Release release);
        Task ShowAlreadyRunningDialog();
        void SetLifetime(IApplicationLifetime applicationLifetime);
        void Shutdown(int exitCode);
        Task ShowWhatIsNewDialog(Release release);
    }
}

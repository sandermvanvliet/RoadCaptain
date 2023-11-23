// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.Shared
{
    public interface IWindowService
    {
        Task ShowErrorDialog(string message);
        Task ShowErrorDialog(string message, Window? owner);
        void SetLifetime(IApplicationLifetime applicationLifetime);
        void Shutdown(int exitCode);
        Window? CurrentWindow { get; }
        Task ShowAlreadyRunningDialog(string applicationName);
        Task<string?> ShowOpenFileDialog(string? previousLocation, IDictionary<string, string> filters);
        Task ShowNewVersionDialog(Release release);
        Task ShowWhatIsNewDialog(Release release);
        Task<RouteModel?> ShowSelectRouteDialog();
        Task<TokenResponse?> ShowLogInDialog(Window owner);
        Window? GetCurrentWindow();
    }
}

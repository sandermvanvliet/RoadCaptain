// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace RoadCaptain.App.Shared
{
    public class DesignTimeWindowService : IWindowService
    {
        public Task ShowErrorDialog(string message)
        {
            return Task.CompletedTask;
        }

        public Task ShowErrorDialog(string message, Window owner)
        {
            throw new System.NotImplementedException();
        }

        public void SetLifetime(IApplicationLifetime applicationLifetime)
        {
            throw new System.NotImplementedException();
        }

        public void Shutdown(int exitCode)
        {
            throw new System.NotImplementedException();
        }

        public Window? CurrentWindow { get; }
        public Task ShowAlreadyRunningDialog(string applicationName)
        {
            throw new System.NotImplementedException();
        }

        public Task<string?> ShowOpenFileDialog(string? previousLocation, IDictionary<string, string> filters)
        {
            throw new System.NotImplementedException();
        }

        public Task ShowNewVersionDialog(Release release)
        {
            throw new System.NotImplementedException();
        }

        public Task ShowWhatIsNewDialog(Release release)
        {
            throw new System.NotImplementedException();
        }

        public Task<RouteModel?> ShowSelectRouteDialog()
        {
            throw new System.NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared.Models;
using RoadCaptain.App.Shared.Views;

namespace RoadCaptain.App.Runner.Tests.Unit.ViewModels
{
    public class StubWindowService : IWindowService
    {
        public StubWindowService(IComponentContext componentContext, MonitoringEvents monitoringEvents)
        {
        }

        public string OpenFileDialogResult { get; set; }
        public TokenResponse LogInDialogResult { get; set; }
        public int OpenFileDialogInvocations { get; private set; }
        public int LogInDialogInvocations { get; private set; }
        public int MainWindowInvocations { get; private set; }
        public int ErrorDialogInvocations { get; private set; }
        public Dictionary<Type, object> Overrides { get; } = new();

        public List<Type> ClosedWindows { get; } = new();
        public List<Type> ShownWindows { get; } = new();

        public async Task<string?> ShowOpenFileDialog(string? previousLocation)
        {
            OpenFileDialogInvocations++;
            return OpenFileDialogResult;
        }

        public void ShowInGameWindow(InGameNavigationWindowViewModel viewModel)
        {
            ShownWindows.Add(typeof(Runner.Views.InGameNavigationWindow));
        }

        public async Task<TokenResponse?> ShowLogInDialog(Window owner)
        {
            LogInDialogInvocations++;
            return LogInDialogResult;
        }

        public Task ShowErrorDialog(string message, Window owner)
        {
            throw new NotImplementedException();
        }

        public async Task ShowErrorDialog(string message)
        {
            ErrorDialogInvocations++;
        }

        public void ShowMainWindow()
        {
            MainWindowInvocations++;
            ShownWindows.Add(typeof(Runner.Views.MainWindow));
        }

        public Task ShowNewVersionDialog(Release release)
        {
            throw new NotImplementedException();
        }

        public Task ShowAlreadyRunningDialog()
        {
            throw new NotImplementedException();
        }

        public void SetLifetime(IApplicationLifetime applicationLifetime)
        {
            throw new NotImplementedException();
        }

        public void Shutdown(int exitCode)
        {
            throw new NotImplementedException();
        }
    }
}
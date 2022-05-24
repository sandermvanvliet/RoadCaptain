using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Avalonia.Controls;
using RoadCaptain.App.Shared.Models;

namespace RoadCaptain.App.Runner.Tests.Unit.ViewModels
{
    public class StubWindowService : WindowService
    {
        public StubWindowService(IComponentContext componentContext, MonitoringEvents monitoringEvents)
            :base(componentContext, monitoringEvents)
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

        protected override void Show(Window window)
        {
            ShownWindows.Add(window.GetType());
            base.Show(window);
        }

        public override async Task ShowErrorDialog(string message)
        {
            ErrorDialogInvocations++;
        }

        public override async Task<string?> ShowOpenFileDialog(string? previousLocation)
        {
            OpenFileDialogInvocations++;
            return OpenFileDialogResult;
        }

        public override async Task<TokenResponse?> ShowLogInDialog(Window owner)
        {
            LogInDialogInvocations++;
            return LogInDialogResult;
        }

        protected override void Close(Window window)
        {
            ClosedWindows.Add(window.GetType());
            base.Close(window);
        }
    }
}
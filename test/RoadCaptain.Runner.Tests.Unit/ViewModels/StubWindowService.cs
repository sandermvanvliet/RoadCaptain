using System;
using System.Collections.Generic;
using System.Windows;
using Autofac;
using Microsoft.Win32;
using RoadCaptain.Runner.Models;

namespace RoadCaptain.Runner.Tests.Unit.ViewModels
{
    public class StubWindowService : WindowService
    {
        public StubWindowService(IComponentContext componentContext) 
            : base(componentContext)
        {
        }

        public string OpenFileDialogResult { get; set; }
        public TokenResponse LogInDialogResult { get; set; }
        public int OpenFileDialogInvocations { get; private set; }
        public int LogInDialogInvocations { get; private set; }
        public int MainWindowInvocations { get; private set; }
        public int ErrorDialogInvocations { get; private set; }
        public Dictionary<Type, object> Overrides { get; } = new();

        protected override bool? ShowDialog(Window window)
        {
            if (window is ZwiftLoginWindow loginWindow)
            {
                LogInDialogInvocations++;
                loginWindow.TokenResponse = LogInDialogResult;
                return true;
            }

            return false;
        }

        protected override bool? ShowDialog(CommonDialog dialog)
        {
            OpenFileDialogInvocations++;

            if (dialog is OpenFileDialog fileDialog && 
                !string.IsNullOrEmpty(OpenFileDialogResult))
            {
                fileDialog.FileName = OpenFileDialogResult;

                return true;
            }

            return false;
        }

        public override void ShowErrorDialog(string message, Window owner)
        {
            ErrorDialogInvocations++;
        }

        protected override void Show(Window window)
        {
            ShownWindows.Add(window.GetType());
        }

        protected override void Close(Window window)
        {
            ClosedWindows.Add(window.GetType());
        }

        public List<Type> ClosedWindows { get; } = new();
        public List<Type> ShownWindows { get; } = new();

        protected override bool Activate(Window window)
        {
            return true;
        }

        protected override TType Resolve<TType>()
        {
            if (Overrides.ContainsKey(typeof(TType)))
            {
                return (TType)Overrides[typeof(TType)];
            }

            return base.Resolve<TType>();
        }
    }
}
using System.Windows;
using RoadCaptain.Runner.Models;
using RoadCaptain.Runner.ViewModels;

namespace RoadCaptain.Runner.Tests.Unit.ViewModels
{
    public class StubWindowService : IWindowService
    {
        public string ShowOpenFileDialog()
        {
            OpenFileDialogInvocations++;

            return OpenFileDialogResult;
        }

        public string OpenFileDialogResult { get; set; }
        public TokenResponse LogInDialogResult { get; set; }

        public int OpenFileDialogInvocations { get; private set; }
        public int LogInDialogInvocations { get; private set; }
        public int MainWindowInvocations { get; private set; }
        public int ErrorDialogInvocations { get; private set; }

        public void ShowInGameWindow(Window owner, InGameNavigationWindowViewModel viewModel)
        {
            throw new System.NotImplementedException();
        }

        public TokenResponse ShowLogInDialog(Window owner)
        {
            LogInDialogInvocations++;

            return LogInDialogResult;
        }

        public void ShowErrorDialog(string message, Window owner = null)
        {
            ErrorDialogInvocations++;
        }

        public void ShowMainWindow()
        {
            MainWindowInvocations++;
        }
    }
}
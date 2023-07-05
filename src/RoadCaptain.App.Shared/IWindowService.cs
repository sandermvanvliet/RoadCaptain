using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace RoadCaptain.App.Shared
{
    public interface IWindowService
    {
        Task ShowErrorDialog(string message);
        Task ShowErrorDialog(string message, Window owner);
        void SetLifetime(IApplicationLifetime applicationLifetime);
        void Shutdown(int exitCode);
        Window? CurrentWindow { get; }
        Task ShowAlreadyRunningDialog(string applicationName);
        Task<string?> ShowOpenFileDialog(string? previousLocation, IDictionary<string, string> filters);
        Task ShowNewVersionDialog(Release release);
        Task ShowWhatIsNewDialog(Release release);
    }
}
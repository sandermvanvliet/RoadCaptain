using System;
using ReactiveUI;

namespace RoadCaptain.App.Shared.Dialogs.ViewModels
{
    public class DesignTimeUpdateAvailableViewModel : UpdateAvailableViewModel
    {
        public DesignTimeUpdateAvailableViewModel(): base(new Release { InstallerDownloadUri = new Uri("https://tempuri.org"), IsPreRelease = true, ReleaseNotes = "lorem ipsum, dolor sit amet", Version = System.Version.Parse("0.1.2.3")})
        {
        }
    }
}
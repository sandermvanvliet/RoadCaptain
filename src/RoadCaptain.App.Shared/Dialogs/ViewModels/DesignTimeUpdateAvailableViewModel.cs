// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain.App.Shared.Dialogs.ViewModels
{
    public class DesignTimeUpdateAvailableViewModel : UpdateAvailableViewModel
    {
        public DesignTimeUpdateAvailableViewModel(): base(new Release(version: System.Version.Parse("0.1.2.3"), installerDownloadUri: new Uri("https://tempuri.org"), isPreRelease: true, releaseNotes: "lorem ipsum, dolor sit amet"))
        {
        }
    }
}

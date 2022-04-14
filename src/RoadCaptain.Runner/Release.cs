using System;

namespace RoadCaptain.Runner
{
    public class Release
    {
        public Version Version { get; set; }
        public string ReleaseNotes { get; set; }
        public Uri InstallerDownloadUri { get; set; }
    }
}
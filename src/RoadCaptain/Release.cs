// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain
{
    public class Release
    {
        public Release(Version version, Uri installerDownloadUri, bool isPreRelease, string releaseNotes)
        {
            InstallerDownloadUri = installerDownloadUri;
            IsPreRelease = isPreRelease;
            ReleaseNotes = releaseNotes;
            Version = version;
        }

        public Version Version { get; }
        public string ReleaseNotes { get; }
        public Uri InstallerDownloadUri { get; }
        public bool IsPreRelease { get; }
    }
}

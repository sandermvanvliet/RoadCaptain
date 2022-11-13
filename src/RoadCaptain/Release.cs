// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain
{
    public class Release
    {
        public Version Version { get; set; }
        public string ReleaseNotes { get; set; }
        public Uri InstallerDownloadUri { get; set; }
        public bool IsPreRelease { get; set; }
    }
}

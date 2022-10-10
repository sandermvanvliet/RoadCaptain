// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Drawing;

namespace RoadCaptain.Tests.Unit
{
    public class TestUserPreferences : IUserPreferences
    {
        public string? DefaultSport { get; set; }
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }
        public CapturedWindowLocation? InGameWindowLocation { get; set; }
        public bool EndActivityAtEndOfRoute { get; set; }
        public Version LastOpenedVersion { get; set; }
        public byte[]? ConnectionSecret { get; set; }
        public CapturedWindowLocation? RouteBuilderLocation { get; set; }

        public void Load()
        {
        }

        public void Save()
        {
        }
    }
}

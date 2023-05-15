// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain.App.RouteBuilder.Tests.Unit
{
    public class TestUserPreferences : IUserPreferences
    {
        public string? DefaultSport { get; set; } = "Cycling";
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }
        public CapturedWindowLocation? InGameWindowLocation { get; set; }
        public bool EndActivityAtEndOfRoute { get; set; }
        public Version? LastOpenedVersion { get; set; }
        public byte[]? ConnectionSecret { get; set; }
        public CapturedWindowLocation? RouteBuilderLocation { get; set; }
        public bool ShowSprints { get; set; }
        public bool ShowClimbs { get; set; }
        public bool ShowElevationPlot { get; set; }
        public CapturedWindowLocation? ElevationPlotWindowLocation { get; set; }
        public bool ShowElevationPlotInGame { get; set; }
        public int? ElevationPlotRangeInMeters { get; set; }
        public bool? ElevationProfileZoomOnPosition { get; set; }

        public void Load()
        {
        }

        public void Save()
        {
        }
    }
}

// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain
{
    public interface IUserPreferences
    {
        string? DefaultSport { get; set; }
        string? LastUsedFolder { get; set; }
        string? Route { get; set; }
        CapturedWindowLocation? InGameWindowLocation { get; set; }
        bool EndActivityAtEndOfRoute { get; set; }
        Version? LastOpenedVersion { get; set; }
        byte[]? ConnectionSecret { get; }
        CapturedWindowLocation? RouteBuilderLocation { get; set; }
        bool ShowSprints { get; set; }
        bool ShowClimbs { get; set; }
        bool ShowElevationProfile { get; set; }
        CapturedWindowLocation? ElevationProfileWindowLocation { get; set; }
        bool ShowElevationProfileInGame { get; set; }
        string? ElevationProfileRenderMode { get; set; }
        void Load();
        void Save();
    }
}

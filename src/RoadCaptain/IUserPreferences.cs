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
        Version LastOpenedVersion { get; set; }
        byte[]? ConnectionSecret { get; }
        CapturedWindowLocation? RouteBuilderLocation { get; set; }
        void Load();
        void Save();
    }
}
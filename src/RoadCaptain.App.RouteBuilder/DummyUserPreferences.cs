using System;

namespace RoadCaptain.App.RouteBuilder
{
    internal class DummyUserPreferences : IUserPreferences
    {
        public string? DefaultSport { get; set; } = "Cycling";
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }
        public CapturedWindowLocation? InGameWindowLocation { get; set; } = new(600, 200, false, 800, 600);
        public bool EndActivityAtEndOfRoute { get; set; }
        public Version LastOpenedVersion { get; set; } = new Version(0, 0, 0, 0);

        public byte[]? ConnectionSecret { get; }
        public CapturedWindowLocation? RouteBuilderLocation { get; set; }

        public void Load()
        {
        }

        public void Save()
        {
        }
    }
}
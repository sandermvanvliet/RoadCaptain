using System;
using System.Drawing;
using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.Runner
{
    public class DummyUserPreferences : IUserPreferences
    {
        public string? DefaultSport { get; set; } = "Cycling";
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }
        public Point? InGameWindowLocation { get; set; }
        public bool EndActivityAtEndOfRoute { get; set; }
        public bool LoopRouteAtEndOfRoute { get; set; }
        public Version LastOpenedVersion { get; set; } = typeof(DummyUserPreferences).Assembly.GetName().Version;
        public byte[]? ConnectionSecret { get; }

        public void Load()
        {
        }

        public void Save()
        {
        }
    }
}
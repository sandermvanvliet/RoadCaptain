using System;
using System.Drawing;
using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.RouteBuilder
{
    internal class DummyUserPreferences : IUserPreferences
    {
        public string? DefaultSport { get; set; } = "Cycling";
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }
        public Point? InGameWindowLocation { get; set; } = new Point(600, 200);
        public bool EndActivityAtEndOfRoute { get; set; }
        public Version LastOpenedVersion { get; set; } = new Version(0, 0, 0, 0);

        public byte[]? ConnectionSecret { get; }

        public void Load()
        {
        }

        public void Save()
        {
        }
    }
}
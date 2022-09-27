using System;
using System.Drawing;

namespace RoadCaptain.Tests.Unit
{
    public class TestUserPreferences : IUserPreferences
    {
        public string? DefaultSport { get; set; }
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }
        public Point? InGameWindowLocation { get; set; }
        public bool EndActivityAtEndOfRoute { get; set; }
        public Version LastOpenedVersion { get; set; }
        public byte[]? ConnectionSecret { get; set; }
        public void Load()
        {
        }

        public void Save()
        {
        }
    }
}
using System;
using System.Drawing;

namespace RoadCaptain
{
    public interface IUserPreferences
    {
        public string? DefaultSport { get; set; }
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }
        Point? InGameWindowLocation { get; set; }
        public bool EndActivityAtEndOfRoute { get; set; }
        public bool LoopRouteAtEndOfRoute { get; set; }
        Version LastOpenedVersion { get; set; }
        public byte[]? ConnectionSecret { get; }
        public void Load();
        public void Save();
    }
}
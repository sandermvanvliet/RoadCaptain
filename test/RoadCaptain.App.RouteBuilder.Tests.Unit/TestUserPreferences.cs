using System.Drawing;
using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.RouteBuilder.Tests.Unit
{
    public class TestUserPreferences : IUserPreferences
    {
        public string? DefaultSport { get; set; } = "Cycling";
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }
        public Point? InGameWindowLocation { get; set; }
        public void Load()
        {
        }

        public void Save()
        {
        }
    }
}
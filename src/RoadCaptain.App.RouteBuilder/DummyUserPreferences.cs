using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.RouteBuilder
{
    internal class DummyUserPreferences : IUserPreferences
    {
        public string? DefaultSport { get; set; } = "Cycling";
        public string? LastUsedFolder { get; set; }
        public void Load()
        {
        }

        public void Save()
        {
        }
    }
}
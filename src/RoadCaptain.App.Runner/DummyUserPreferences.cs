using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.Runner
{
    public class DummyUserPreferences : IUserPreferences
    {
        public string? DefaultSport { get; set; } = "Cycling";
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }

        public void Load()
        {
        }

        public void Save()
        {
        }
    }
}
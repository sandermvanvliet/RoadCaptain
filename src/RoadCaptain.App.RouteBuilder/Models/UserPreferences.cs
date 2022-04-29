namespace RoadCaptain.App.RouteBuilder.Models
{
    public class UserPreferences
    {
        public string? LastUsedFolder { get; set; }
        public string? DefaultSport { get; set; }

        public void Save()
        {
            // TODO: Implement preferences saving
        }
    }
}
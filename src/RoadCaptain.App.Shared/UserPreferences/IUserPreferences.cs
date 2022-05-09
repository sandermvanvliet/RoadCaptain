namespace RoadCaptain.App.Shared.UserPreferences
{
    public interface IUserPreferences
    {
        public string? DefaultSport { get; set; }
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }

        public void Load();
        public void Save();
    }
}
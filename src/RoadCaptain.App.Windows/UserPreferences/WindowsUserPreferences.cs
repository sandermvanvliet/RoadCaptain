using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.Windows.UserPreferences
{
    internal class WindowsUserPreferences : IUserPreferences
    {
        public string? DefaultSport { get; set; }
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }

        public void Load()
        {
            var userPreferences = WindowsUserPreferencesStorage.Default;
            
            if (userPreferences.UpgradeSettings)
            {
                userPreferences.Upgrade();
                userPreferences.UpgradeSettings = false;
                userPreferences.Save();
            }

            DefaultSport = userPreferences.DefaultSport;
            LastUsedFolder = userPreferences.LastUsedFolder;
        }

        public void Save()
        {
            var userPreferences = WindowsUserPreferencesStorage.Default;
            userPreferences.DefaultSport = DefaultSport;
            userPreferences.LastUsedFolder = LastUsedFolder;
            userPreferences.Save();
        }
    }
}
using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.MacOs.UserPreferences
{
    internal class MacOsUserPreferences : UserPreferencesBase
    {
        protected override void EnsureConfigDirectoryExists()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var directory = Path.Combine(home, "Library", "RoadCaptain");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        protected override string GetPreferencesPath()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var configDirectory = Path.Combine(home, "Library", "RoadCaptain");
            
            return Path.Combine(configDirectory, "Configuration");
        }
    }
}
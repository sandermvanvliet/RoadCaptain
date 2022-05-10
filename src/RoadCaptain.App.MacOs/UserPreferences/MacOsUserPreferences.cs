using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.MacOs.UserPreferences
{
    internal class MacOsUserPreferences : UserPreferencesBase
    {
        protected override string GetPreferencesPath()
        {
            var configDirectory = Path.Combine("~", "Library", "RoadCaptain");
            
            return Path.Combine(configDirectory, "Configuration");
        }
    }
}
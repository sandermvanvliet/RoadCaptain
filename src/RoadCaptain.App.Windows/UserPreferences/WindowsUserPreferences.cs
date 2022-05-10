using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.Windows.UserPreferences
{
    internal class WindowsUserPreferences : UserPreferencesBase
    {
        protected override string GetPreferencesPath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            return Path.Combine(appDataPath, "Codenizer BV", "RoadCaptain", "userpreferences.json");
        }
    }
}
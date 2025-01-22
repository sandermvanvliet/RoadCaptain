// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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

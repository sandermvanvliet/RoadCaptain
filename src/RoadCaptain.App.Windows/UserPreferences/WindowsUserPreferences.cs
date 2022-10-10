// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.Windows.UserPreferences
{
    internal class WindowsUserPreferences : UserPreferencesBase
    {
        protected override void EnsureConfigDirectoryExists()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var directory = Path.Combine(appDataPath, "Codenizer BV");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            directory = Path.Combine(directory, "RoadCaptain");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        protected override string GetPreferencesPath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            return Path.Combine(appDataPath, "Codenizer BV", "RoadCaptain", "userpreferences.json");
        }
    }
}

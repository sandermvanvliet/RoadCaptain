// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.Linux.UserPreferences
{
    internal class LinuxUserPreferences : UserPreferencesBase
    {
        protected override void EnsureConfigDirectoryExists()
        {
            var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");

            if (string.IsNullOrEmpty(xdgConfigHome))
            {
                xdgConfigHome = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".config");
            }

            if (!Directory.Exists(xdgConfigHome))
            {
                Directory.CreateDirectory(xdgConfigHome);
            }

            var directory = Path.Combine(xdgConfigHome, "roadcaptain");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        protected override string GetPreferencesPath()
        {
            var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");

            if (string.IsNullOrEmpty(xdgConfigHome))
            {
                xdgConfigHome = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".config");
            }

            return Path.Combine(xdgConfigHome, "roadcaptain", "config");
        }
    }
}

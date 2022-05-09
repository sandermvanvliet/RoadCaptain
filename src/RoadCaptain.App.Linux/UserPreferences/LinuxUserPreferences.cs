using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.Linux.UserPreferences
{
    internal class LinuxUserPreferences : IUserPreferences
    {
        public string? DefaultSport { get; set; }
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }

        public void Load()
        {
            var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");

            if (string.IsNullOrEmpty(xdgConfigHome))
            {
                xdgConfigHome = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".config");
            }

            var configPath = Path.Combine(xdgConfigHome, "roadcaptain", "config");

            if (File.Exists(configPath))
            {
                var settings = File
                    .ReadAllLines(configPath)
                    .Select(line => line.Trim())
                    .Where(line => !line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Split('=', StringSplitOptions.RemoveEmptyEntries))
                    .Where(parts => parts.Length == 2)
                    .ToDictionary(parts => parts[0], parts => parts[1]);

                if (settings.ContainsKey(nameof(DefaultSport)))
                {
                    DefaultSport = settings[nameof(DefaultSport)];
                }

                if (settings.ContainsKey(nameof(LastUsedFolder)))
                {
                    DefaultSport = settings[nameof(LastUsedFolder)];
                }
            }
        }

        public void Save()
        {
            var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");

            if (string.IsNullOrEmpty(xdgConfigHome))
            {
                xdgConfigHome = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".config");
            }

            xdgConfigHome = Path.Combine(xdgConfigHome, "roadcaptain");

            if (!Directory.Exists(xdgConfigHome))
            {
                Directory.CreateDirectory(xdgConfigHome);
            }            

            var configPath = Path.Combine(xdgConfigHome, "config");

            File.WriteAllLines(configPath, new[]
            {
                $"{nameof(DefaultSport)}={DefaultSport??string.Empty}",
                $"{nameof(LastUsedFolder)}={LastUsedFolder??string.Empty}"
            });
        }
    }
}
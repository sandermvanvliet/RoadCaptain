using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.MacOs.UserPreferences
{
    internal class MacOsUserPreferences : IUserPreferences
    {
        public string? DefaultSport { get; set; }
        public string? LastUsedFolder { get; set; }
        public void Load()
        {
            var configDirectory = Path.Combine("~", "Library", "RoadCaptain");
            
            var configPath = Path.Combine(configDirectory, "Configuration");

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
            var configDirectory = Path.Combine("~", "Library", "RoadCaptain");

            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }

            var configPath = Path.Combine(configDirectory, "Configuration");

            File.WriteAllLines(configPath, new[]
            {
                $"{nameof(DefaultSport)}={DefaultSport??string.Empty}",
                $"{nameof(LastUsedFolder)}={LastUsedFolder??string.Empty}"
            });
        }
    }
}
using System;
using Newtonsoft.Json;

namespace RoadCaptain.App.Shared.UserPreferences
{
    internal class UserPreferencesStorageObject
    {
        public string? DefaultSport { get; set; }
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }
        [JsonConverter(typeof(CapturedWindowLocationConverter))]
        public CapturedWindowLocation? InGameWindowLocation { get; set; }
        public Version? LastOpenedVersion { get; set; }
        [JsonConverter(typeof(CapturedWindowLocationConverter))]
        public CapturedWindowLocation? RouteBuilderWindowLocation { get; set; }
    }
}
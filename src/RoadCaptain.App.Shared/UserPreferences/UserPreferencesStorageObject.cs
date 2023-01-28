// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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

        public bool ShowClimbs { get; set; }
        public bool ShowSprints { get; set; }
        public bool ShowElevationPlot { get; set; }
    }
}

// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Newtonsoft.Json;

namespace RoadCaptain.Adapters
{
    internal class PersistedRouteVersion3
    {
        [JsonProperty("version")]
        public const string? Version = "3";

        [JsonProperty("roadCaptainVersion")]
        public string? RoadCaptainVersion { get; set; }

        [JsonProperty("route")]
        public PlannedRoute? Route { get; set; }
    }
}

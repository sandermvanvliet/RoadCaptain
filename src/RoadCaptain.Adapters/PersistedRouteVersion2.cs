// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Newtonsoft.Json;

namespace RoadCaptain.Adapters
{
    internal class PersistedRouteVersion2
    {
        [JsonProperty("version")]
        public const string Version = "2";

        [JsonProperty("roadCaptainVersion")]
        public string RoadCaptainVersion { get; set; }

        [JsonProperty("route")]
        public PlannedRoute Route { get; set; }
    }
}

// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Newtonsoft.Json;

namespace RoadCaptain.Adapters
{
    internal class PersistedRouteVersion1
    {
        [JsonProperty("version")]
        public const string Version = "1";

        [JsonProperty("route")]
        public PlannedRoute Route { get; set; }
    }
}

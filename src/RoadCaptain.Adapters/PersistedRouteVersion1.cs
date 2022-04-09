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
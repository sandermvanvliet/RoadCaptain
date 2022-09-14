using Newtonsoft.Json;

namespace RoadCaptain.Adapters
{
    internal class PersistedRouteVersion3
    {
        [JsonProperty("version")]
        public const string Version = "3";

        [JsonProperty("roadCaptainVersion")]
        public string RoadCaptainVersion { get; set; }

        [JsonProperty("route")]
        public PlannedRoute Route { get; set; }
    }
}
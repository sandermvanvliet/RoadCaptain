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
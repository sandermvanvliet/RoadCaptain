using Newtonsoft.Json;

namespace RoadCaptain
{
    public class World
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SpawnPoint[] SpawnPoints { get; set; }
        public WorldStatus Status { get; set; }
        [JsonIgnore]
        public string Image => $"pack://application:,,,/RoadCaptain.UserInterface.Shared;component/Assets/world-{Id}.jpg";
    }

    public enum WorldStatus
    {
        Unknown,
        Available,
        Unavailable
    }
}

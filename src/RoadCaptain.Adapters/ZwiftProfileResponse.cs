using Newtonsoft.Json;

namespace RoadCaptain.Adapters
{
    internal class ZwiftProfileResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("publicId")]
        public string PublicId { get; set; }
        [JsonProperty("riding")]
        public bool Riding { get; set; }
        [JsonProperty("likelyInGame")]
        public bool LikelyInGame { get; set; }
        [JsonProperty("worldId")]
        public int? WorldId { get; set; }

        public ZwiftProfile ToDomain()
        {
            return new ZwiftProfile
            {
                Id = Id,
                PublicId = PublicId,
                WorldId = WorldId,
                Riding = Riding,
                LikelyInGame = LikelyInGame
            };
        }
    }
}
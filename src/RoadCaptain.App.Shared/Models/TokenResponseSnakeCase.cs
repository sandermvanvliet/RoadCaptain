using System.Text.Json.Serialization;

namespace RoadCaptain.App.Shared.Models
{
    public class TokenResponseSnakeCase
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("expires_in")]
        public long ExpiresIn { get; set; }
        [JsonPropertyName("userProfile")]
        public UserProfile UserProfile { get; set; }
    }
}
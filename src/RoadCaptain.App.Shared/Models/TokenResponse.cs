using System.Text.Json.Serialization;

namespace RoadCaptain.App.Shared.Models
{
    public class TokenResponse
    {
        [JsonPropertyName("accessToken")]
        public string? AccessToken { get; set; }
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("expiresIn")]
        public long ExpiresIn { get; set; }
        [JsonPropertyName("userProfile")]
        public UserProfile? UserProfile { get; set; }
    }
}
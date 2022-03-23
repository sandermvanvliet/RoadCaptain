using System.Text.Json.Serialization;

namespace RoadCaptain.Runner.Models
{
    public class TokenResponse
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

    public class UserProfile
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
        [JsonPropertyName("avatar")] 
        public string Avatar { get; set; }
    }
}
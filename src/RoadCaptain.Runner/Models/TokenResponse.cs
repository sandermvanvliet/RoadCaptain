// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Text.Json.Serialization;

namespace RoadCaptain.Runner.Models
{
    public class TokenResponse
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("expiresIn")]
        public long ExpiresIn { get; set; }
        [JsonPropertyName("userProfile")]
        public UserProfile UserProfile { get; set; }
    }

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

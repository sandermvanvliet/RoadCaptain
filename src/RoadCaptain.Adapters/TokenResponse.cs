// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Newtonsoft.Json;

namespace RoadCaptain.Adapters
{
    internal class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("refresh_expires_in")]
        public int RefreshExpiresIn { get; set; }

        public OAuthToken ToDomain()
        {
            return new OAuthToken
            {
                AccessToken = AccessToken,
                RefreshToken = RefreshToken,
                ExpiresOn = DateTime.UtcNow.AddSeconds(ExpiresIn),
                RefreshTokenExpiresOn = DateTime.UtcNow.AddSeconds(RefreshExpiresIn)
            };
        }
    }
}

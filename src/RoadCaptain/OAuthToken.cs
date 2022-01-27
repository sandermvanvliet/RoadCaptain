using System;

namespace RoadCaptain
{
    public class OAuthToken
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresOn { get; set; }
        public DateTime RefreshTokenExpiresOn { get; set; }
    }
}
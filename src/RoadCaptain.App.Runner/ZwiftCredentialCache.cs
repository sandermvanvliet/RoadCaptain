using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;
using RoadCaptain.App.Shared.Models;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner
{
    public interface IZwiftCredentialCache
    {
        Task StoreAsync(TokenResponse tokenResponse);
        Task<TokenResponse?> LoadAsync();
    }

    public class ZwiftCredentialCache : IZwiftCredentialCache
    {
        private TokenResponse? _cachedCredentials;
        private readonly IZwift _zwift;

        public ZwiftCredentialCache(IZwift zwift)
        {
            _zwift = zwift;
        }

        public async Task StoreAsync(TokenResponse tokenResponse)
        {
            _cachedCredentials = tokenResponse;

#if DEBUG
            // This is for testing only to prevent me having to log in all the time.
            if (tokenResponse.UserProfile.FirstName + " " + tokenResponse.UserProfile.LastName ==
                "Sander van Vliet [RoadCaptain]")
            {
                await File.WriteAllTextAsync("devtokens.json", JsonConvert.SerializeObject(tokenResponse, Formatting.Indented));
            }
#endif
        }

        public async Task<TokenResponse?> LoadAsync()
        {
            var tokenResponse = _cachedCredentials;

#if DEBUG
            // This is for testing only to prevent me having to log in all the time.
            if (tokenResponse == null && File.Exists("devtokens.json"))
            {
                tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(await File.ReadAllTextAsync("devtokens.json"));

                if (tokenResponse?.AccessToken != null && new JsonWebToken(tokenResponse.AccessToken).ValidTo < DateTime.Now)
                {
                    // When the token expires, break here and use postman to refresh the token manually
                    var oauthToken = await _zwift.RefreshTokenAsync(tokenResponse.RefreshToken);

                    if (oauthToken != null)
                    {
                        tokenResponse.AccessToken = oauthToken.AccessToken;
                        tokenResponse.RefreshToken = oauthToken.RefreshToken;
                        tokenResponse.ExpiresIn = (int)oauthToken.ExpiresOn.Subtract(DateTime.UtcNow).TotalSeconds;
                    }
                    else
                    {
                        tokenResponse = null;
                    }
                }
            }
#endif

            return tokenResponse;
        }
    }
}
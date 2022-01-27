using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    public class RequestTokenFromApi : IRequestToken
    {
        private readonly HttpClient _httpClient;

        public RequestTokenFromApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<OAuthToken> RequestAsync(string username, string password)
        {
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("username", username),
                new("password", password),
                new("client_id", "Zwift_Mobile_Link"),
                new("grant_type", "password")
            };

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://secure.zwift.com/auth/realms/zwift/protocol/openid-connect/token"));
            request.Content = new FormUrlEncodedContent(parameters);
            request.Headers.Add("Zwift-Api-Version", "2.6");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to retrieve token: " + response.StatusCode);
            }
            
            var serializedContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(serializedContent);

            return new OAuthToken
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                ExpiresOn = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                RefreshTokenExpiresOn = DateTime.UtcNow.AddSeconds(tokenResponse.RefreshExpiresIn)
            };
        }
    }
}
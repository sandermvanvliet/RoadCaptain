// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class Zwift : IZwift
    {
        private const string SecureRelayRequestPayload = "{\"secret\":\"##SECRET##\",\"mobileEnvironment\":{\"systemHardware\":\"iPhone14,2\",\"appBuild\":1258,\"systemOSVersion\":\"15.5\",\"appVersion\":\"3.37.0\",\"systemOS\":\"iOS\",\"appDisplayName\":\"Companion\"},\"phoneAddress\":\"##IP##\"}";
        private readonly HttpClient _httpClient;

        public Zwift(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Uri> RetrieveRelayUrl(string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://us-or-rly101.zwift.com/api/servers"));
            request.Headers.Add("Zwift-Api-Version", "2.6");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to retrieve relay URL: " + response.StatusCode);
            }

            var responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());

            return new Uri(responseObject["baseUrl"].Value<string>());
        }

        public async Task InitiateRelayAsync(string accessToken, Uri uri, string ipAddress, byte[] connectionSecret)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(uri, "/relay/profiles/me/phone"));
            request.Headers.Add("Zwift-Api-Version", "2.6");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var encodedConnectionSecret = Convert.ToBase64String(connectionSecret);

            request.Content = new StringContent(
                SecureRelayRequestPayload
                    .Replace("##IP##", ipAddress)
                    .Replace("##SECRET##", encodedConnectionSecret),
                null);
            
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to retrieve token: " + response.StatusCode);
            }
        }

        public async Task<ZwiftProfile> GetProfileAsync(string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://us-or-rly101.zwift.com/api/profiles/me/"));
            request.Headers.Add("Zwift-Api-Version", "2.6");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to retrieve profile: " + response.StatusCode);
            }

            var serializedContent = await response.Content.ReadAsStringAsync();
            var profile = JsonConvert.DeserializeObject<ZwiftProfileResponse>(serializedContent);

            return profile.ToDomain();
        }

        public async Task<OAuthToken> RefreshTokenAsync(string refreshToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://secure.zwift.com/auth/realms/zwift/protocol/openid-connect/token");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            request.Content = new FormUrlEncodedContent(new []
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", "zwift-public"),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
            });

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to refresh token: " + response.StatusCode);
            }

            var serializedContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TokenResponse>(serializedContent)?.ToDomain();
        }
    }
}

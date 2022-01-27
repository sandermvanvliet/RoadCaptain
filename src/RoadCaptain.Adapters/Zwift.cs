using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    public class Zwift : IZwift
    {
        private const string RelayRequestPayload = "{\"mobileEnvironment\":{\"appBuild\":1276,\"appDisplayName\":\"Companion\",\"appVersion\":\"3.29.0\",\"systemHardware\":\"Google sdk_gphone_x86_64\",\"systemOS\":\"Android\",\"systemOSVersion\":\"11 (API 30)\"},\"phoneAddress\":\"##IP##\",\"port\":21587,\"protocol\":\"TCP\"}";
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

        public async Task InitiateRelayAsync(string accessToken, Uri uri, string ipAddress)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(uri, "/relay/profiles/me/phone"));
            request.Headers.Add("Zwift-Api-Version", "2.6");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            request.Content = new StringContent(
                RelayRequestPayload.Replace("##IP##", ipAddress),
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
    }
}
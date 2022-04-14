using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RoadCaptain.Runner
{
    public class VersionChecker
    {
        public static readonly JsonSerializerSettings SerializerSettings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        private readonly HttpClient _client;

        public VersionChecker(HttpClient client)
        {
            _client = client;
        }

        public Release GetLatestRelease()
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://api.github.com/repos/sandermvanvliet/RoadCaptain/releases/latest");

                // Use this specific MIME type to get the full payload of the response.
                // application/json only gives us the URL and not much else.
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

                // Github requires a User-Agent header on all requests
                request.Headers.UserAgent.Add(new ProductInfoHeaderValue("RoadCaptain",
                    (GetType().Assembly.GetName().Version ?? new Version(0, 0)).ToString(4)));

                var result = _client.SendAsync(request).GetAwaiter().GetResult();

                if (result.IsSuccessStatusCode)
                {
                    var serialized = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    var release = JsonConvert.DeserializeObject<ReleaseResponse>(serialized, SerializerSettings);

                    if (release != null)
                    {
                        return new Release
                        {
                            Version = Version.Parse(release.TagName),
                            ReleaseNotes = release.Body,
                            InstallerDownloadUri = GetInstallerUriFrom(release)
                        };
                    }
                }
            }
            catch
            {
                // Ignore
            }

            return new Release
            {
                Version = GetType().Assembly.GetName().Version ?? new Version(0, 0)
            };
        }

        private static Uri GetInstallerUriFrom(ReleaseResponse release)
        {
            var installerAsset = release
                .Assets
                .FirstOrDefault(a =>
                    "application/x-msi".Equals(a.ContentType, StringComparison.InvariantCultureIgnoreCase));

            if (installerAsset != null)
            {
                return new Uri(installerAsset.BrowserDownloadUrl);
            }

            return null;
        }
    }
}
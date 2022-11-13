// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class VersionChecker : IVersionChecker
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

        public (Release? official, Release? preRelease) GetLatestRelease()
        {
            return GetLatestReleaseIncludingPreReleases();
        }

        private (Release? official, Release? preRelease) GetLatestReleaseIncludingPreReleases()
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://api.github.com/repos/sandermvanvliet/RoadCaptain/releases");

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

                    var releases = JsonConvert.DeserializeObject<ReleaseResponse[]>(serialized, SerializerSettings);

                    if (releases != null && releases.Any())
                    {
                        var firstOfficial = releases.Where(r => !r.Draft && !r.PreRelease).MaxBy(r => r.CreatedAt);
                        var firstPreRelease = releases.Where(r => !r.Draft && r.PreRelease).MaxBy(r => r.CreatedAt);

                        var officialVersion = firstOfficial == null ? Version.Parse("0.0.0.0") : Version.Parse(firstOfficial.Name);
                        var preReleaseVersion = firstPreRelease == null ? Version.Parse("0.0.0.0") : Version.Parse(firstPreRelease.Name);

                        if (preReleaseVersion < officialVersion)
                        {
                            firstPreRelease = null;
                        }

                        return (FromGitHubRelease(firstOfficial), FromGitHubRelease(firstPreRelease));
                    }
                }
            }
            catch
            {
                // Ignore
            }

            return (
                new Release
                {
                    Version = GetType().Assembly.GetName().Version ?? new Version(0, 0)
                },
                null);
        }

        private Release? FromGitHubRelease(ReleaseResponse? release)
        {
            if (release == null)
            {
                return null;
            }

            return new Release
            {
                Version = Version.Parse(release.Name),
                ReleaseNotes = release.Body,
                InstallerDownloadUri = GetInstallerUriFrom(release),
                IsPreRelease = release.PreRelease
            };
        }

        private static Uri? GetInstallerUriFrom(ReleaseResponse release)
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

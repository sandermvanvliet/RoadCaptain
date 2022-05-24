using System;
using System.Net;
using System.Net.Http;
using Codenizer.HttpClient.Testable;
using FluentAssertions;
using Newtonsoft.Json;
using RoadCaptain.Adapters;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit
{
    public class WhenCheckingForLatestVersion
    {
        private static readonly Version
            CurrentVersion = typeof(WhenCheckingForLatestVersion).Assembly.GetName().Version;

        private static readonly Version NewVersion = new(1, 2, 3, 4);
        private readonly TestableMessageHandler _handler;

        public WhenCheckingForLatestVersion()
        {
            _handler = new TestableMessageHandler();
        }

        [Fact]
        public void GivenGitHubCantBeReached_CurrentVersionIsLatest()
        {
            var result = GetLatestVersion();

            result
                .Version
                .Should()
                .Be(CurrentVersion);
        }

        [Fact]
        public void GivenLatestReleaseOnGithubIsCurrentVersion_CurrentVersionIsReturned()
        {
            GivenGithubRelease(CurrentVersion, "empty body");

            var result = GetLatestVersion();

            result
                .Version
                .Should()
                .Be(CurrentVersion);
        }

        [Fact]
        public void GivenLatestReleaseOnGithubIsNewVersion_NewVersionIsReturned()
        {
            GivenGithubRelease(NewVersion, "empty body");

            var result = GetLatestVersion();

            result
                .Version
                .Should()
                .Be(NewVersion);
        }

        [Fact]
        public void GivenLatestReleaseOnGithubIsNewVersion_DownloadUriForNewVersionIsReturned()
        {
            GivenGithubRelease(NewVersion, "empty body");

            var result = GetLatestVersion();

            result
                .InstallerDownloadUri
                .Should()
                .Be(
                    $"https://github.com/sandermvanvliet/RoadCaptain/releases/download/0.5.4.0/RoadCaptain_{NewVersion}.msi");
        }

        [Fact]
        public void GivenLatestReleaseOnGithubIsNewVersion_ReleaseTextIsReturned()
        {
            GivenGithubRelease(NewVersion, "empty body");

            var result = GetLatestVersion();

            result
                .ReleaseNotes
                .Should()
                .Be("empty body");
        }

        private void GivenGithubRelease(Version version, string body)
        {
            var release = new ReleaseResponse
            {
                TagName = version.ToString(4),
                Body = body,
                Assets = new[]
                {
                    new ReleaseAsset
                    {
                        Id = "001",
                        Name = "Some other asset",
                        ContentType = "text/plain"
                    },
                    new ReleaseAsset
                    {
                        Id = "123",
                        Name = $"RoadCaptain_{version}.msi",
                        BrowserDownloadUrl =
                            $"https://github.com/sandermvanvliet/RoadCaptain/releases/download/0.5.4.0/RoadCaptain_{version}.msi",
                        Url = "https://api.github.com/repos/sandermvanvliet/RoadCaptain/releases/assets/123",
                        ContentType = "application/x-msi"
                    }
                }
            };

            var serializedRelease = JsonConvert.SerializeObject(release, VersionChecker.SerializerSettings);

            _handler
                .RespondTo()
                .Get()
                .ForUrl("/repos/sandermvanvliet/RoadCaptain/releases/latest")
                .Accepting("application/vnd.github.v3+json")
                .With(HttpStatusCode.OK)
                .AndContent("application/vnd.github.v3+json", serializedRelease);
        }

        private Release GetLatestVersion()
        {
            var client = new HttpClient(_handler);
            return new VersionChecker(client)
                .GetLatestRelease();
        }
    }
}
using System.Drawing;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace RoadCaptain.App.Shared.Tests.Unit.UserPreferences
{
    public class WhenLoadingUserPreferences
    {
        [Fact]
        public void GivenUserPreferencesStoredWithPointAsLocation_StoredWindowLocationIsReturned()
        {
            const string serializedPreferences = @"{
  ""defaultSport"": ""Cycling"",
  ""lastUsedFolder"": ""C:\\git\\temp\\zwift\\RoadCaptain-troubleshoot\\102-Italian Villas Rebel Route"",
  ""route"": ""C:\\git\\temp\\zwift\\RoadCaptain-troubleshoot\\102-Italian Villas Rebel Route\\Rebel.Route.-.Italian.Villa.Sprint.Loop.json"",
  ""inGameWindowLocation"": ""1, 6"",
  ""lastOpenedVersion"": ""0.6.8.1""
}";

            var userPreferences = new TestableUserPreferences(serializedPreferences);

            userPreferences.Load();

            userPreferences.InGameWindowLocation?.X
                .Should()
                .Be(1);
        }

        [Fact]
        public void GivenUserPreferencesStoredWithStoredWindowLocationAsLocation_StoredWindowLocationIsReturned()
        {
            const string serializedPreferences = @"{
  ""defaultSport"": ""Cycling"",
  ""lastUsedFolder"": ""C:\\git\\temp\\zwift\\RoadCaptain-troubleshoot\\102-Italian Villas Rebel Route"",
  ""route"": ""C:\\git\\temp\\zwift\\RoadCaptain-troubleshoot\\102-Italian Villas Rebel Route\\Rebel.Route.-.Italian.Villa.Sprint.Loop.json"",
  ""inGameWindowLocation"": { ""x"": 1, ""y"": 6, ""isMaximized"": true },
  ""lastOpenedVersion"": ""0.6.8.1""
}";

            var userPreferences = new TestableUserPreferences(serializedPreferences);

            userPreferences.Load();

            userPreferences.InGameWindowLocation?.X
                .Should()
                .Be(1);
        }
    }
}
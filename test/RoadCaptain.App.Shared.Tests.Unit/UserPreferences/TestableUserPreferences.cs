using RoadCaptain.App.Shared.UserPreferences;

namespace RoadCaptain.App.Shared.Tests.Unit.UserPreferences
{
    public class TestableUserPreferences : UserPreferencesBase
    {
        private readonly string _serializedPreferences;

        public TestableUserPreferences(string serializedPreferences)
        {
            _serializedPreferences = serializedPreferences;
        }

        protected override void EnsureConfigDirectoryExists()
        {
        }

        protected override string GetPreferencesPath()
        {
            return "";
        }

        protected override string? GetFileContents(string preferencesPath)
        {
            return _serializedPreferences;
        }
    }
}
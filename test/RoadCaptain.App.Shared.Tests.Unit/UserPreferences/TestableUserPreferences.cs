// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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

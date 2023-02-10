// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.IO;
using System.Runtime.InteropServices;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Shared
{
    public class PlatformPaths : IPathProvider
    {
        private const string CompanyName = "Codenizer BV";
        private const string ProductName = "RoadCaptain";

        public static string GetUserDataDirectory()
        {
            string localAppDataFolder;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // macOS and Linux
                localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(localAppDataFolder, ProductName);
            }
            
            localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppDataFolder, CompanyName, ProductName);
        }

        string IPathProvider.GetUserDataDirectory()
        {
            return GetUserDataDirectory();
        }
    }
}


﻿using System;
using System.IO;

namespace RoadCaptain.App.Shared
{
    public class PlatformPaths
    {
        private const string CompanyName = "Codenizer BV";
        private const string ProductName = "RoadCaptain";

        public static string LogDirectory()
        {
#if WIN
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logDirectory = Path.Combine(localAppDataFolder, CompanyName, ApplicationName);
#elif MACOS
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var logDirectory = Path.Combine(localAppDataFolder, ApplicationName);
#elif LINUX
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var logDirectory = Path.Combine(localAppDataFolder, ApplicationName);
#else
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logDirectory = Path.Combine(localAppDataFolder, CompanyName, ProductName);
#endif

            return logDirectory;
        }
    }
}
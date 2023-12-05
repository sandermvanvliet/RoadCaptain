// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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

        public static string? RouteBuilderExecutable()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var installationDirectory = Path.GetDirectoryName(assemblyLocation);

            if (!string.IsNullOrEmpty(installationDirectory))
            {
                var executableName = "RoadCaptain.App.RouteBuilder";
                
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    executableName += ".exe";
                }

                if (Debugger.IsAttached)
                {
                    // Because a debug session starts the app from the project
                    // bin folder, that location does not contain the route builder
                    // executable. They only exist side-by-side in a deployed 
                    // situation.
                    installationDirectory =
                        installationDirectory.Replace("RoadCaptain.App.Runner", "RoadCaptain.App.RouteBuilder");
                }
                
                return Path.Combine(installationDirectory, executableName);
            }

            return null;
        }

        string IPathProvider.GetUserDataDirectory()
        {
            return GetUserDataDirectory();
        }

        string? IPathProvider.RouteBuilderExecutable()
        {
            return RouteBuilderExecutable();
        }
    }
}


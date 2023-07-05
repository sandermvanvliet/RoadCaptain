// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.IO;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class LocalDirectoryRouteRepositorySettings
    {
        public LocalDirectoryRouteRepositorySettings(IPathProvider pathProvider)
        {
            Directory = Path.Combine(pathProvider.GetUserDataDirectory(), "Routes");
            IsValid = !string.IsNullOrEmpty(Directory);
        }

        public bool IsValid { get; }

        public string Name => "Local";

        public string Directory { get; }
    }
}

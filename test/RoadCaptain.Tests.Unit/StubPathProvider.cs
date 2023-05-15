// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.IO;
using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    public class StubPathProvider : IPathProvider
    {
        public string GetUserDataDirectory()
        {
            return Path.GetTempPath();
        }
    }
}

// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner.Tests.Unit.ViewModels.MainWindow
{
    public class StubVersionChecker : IVersionChecker
    {
        public (Release? official, Release? preRelease) GetLatestRelease()
        {
            return (new Release(new Version(), new Uri("https://roadcaptain.nl"), false, string.Empty), null);
        }
    }
}

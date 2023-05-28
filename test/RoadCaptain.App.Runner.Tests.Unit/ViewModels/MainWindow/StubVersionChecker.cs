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
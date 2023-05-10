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
using System.IO;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using RoadCaptain.SegmentBuilder;
using Xunit;

namespace RoadCaptain.Tests.Unit
{
    public class OverlapTest
    {
        private readonly TrackPoint _point = new TrackPoint(-11.64775m, 166.94357m, 0.0m);

        [Fact]
        public void Other()
        {
            var route = Route.FromGpxFile(@"C:\git\temp\zwift\zwift-watopia-gpx\watopia-bambino-fondo.gpx");

            var other = route.TrackPoints[2558];
            other.IsCloseTo(_point).Should().BeTrue();
        }
    }
}

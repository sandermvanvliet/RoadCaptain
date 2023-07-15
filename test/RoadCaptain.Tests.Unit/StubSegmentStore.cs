using System.Collections.Generic;
using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    internal class StubSegmentStore : ISegmentStore
    {
        public List<Segment> LoadSegments(World world, SportType sport)
        {
            return new List<Segment>();
        }

        public List<Segment> LoadMarkers(World world)
        {
            return new List<Segment>();
        }
    }
}
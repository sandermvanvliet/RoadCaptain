using System.Collections.Generic;

namespace RoadCaptain.Ports
{
    public interface ISegmentStore
    {
        List<Segment> LoadSegments(World world, SportType sport);
        List<Segment> LoadMarkers(World world);
    }
}

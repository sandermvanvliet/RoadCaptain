using System.Collections.Generic;
using System.Collections.Immutable;

namespace RoadCaptain.SegmentBuilder
{
    internal class Context
    {
        public Context(List<Segment> segments, string gpxDirectory)
        {
            Segments = segments.ToImmutableList();
            GpxDirectory = gpxDirectory;
        }

        public ImmutableList<Segment> Segments { get; }
        public string GpxDirectory { get; }
    }
}
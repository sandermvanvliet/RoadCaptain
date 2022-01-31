using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class SegmentStore : ISegmentStore
    {
        private readonly string _segmentPath;

        public SegmentStore()
        {
            _segmentPath = "segments.json";
        }

        public List<Segment> LoadSegments()
        {
            var segments = JsonConvert.DeserializeObject<List<Segment>>(File.ReadAllText(_segmentPath));

            return segments;
        }
    }
}
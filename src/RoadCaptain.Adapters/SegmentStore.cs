using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class SegmentStore : ISegmentStore
    {
        private readonly string _segmentPath;
        private readonly string _tracksPath;

        public SegmentStore() : this(Environment.CurrentDirectory)
        {
        }

        internal SegmentStore(string fileRoot)
        {
            _segmentPath = Path.Combine(fileRoot, "segments.json");
            _tracksPath = Path.Combine(fileRoot, "turns.json");
        }

        public List<Segment> LoadSegments()
        {
            var segments = JsonConvert.DeserializeObject<List<Segment>>(File.ReadAllText(_segmentPath));
            List<SegmentTurns> turns;
            try
            {
                turns = JsonConvert.DeserializeObject<List<SegmentTurns>>(File.ReadAllText(_tracksPath));
            }
            catch (Exception e)
            {
                Debugger.Break();
                throw;
            }

            foreach (var segment in segments)
            {
                segment.TranslateToGameCoordinates();

                var turnsForSegment = turns.SingleOrDefault(t => t.SegmentId == segment.Id);

                if (turnsForSegment != null)
                {
                    segment.NextSegmentsNodeA.AddRange(turnsForSegment.TurnsA);
                    segment.NextSegmentsNodeB.AddRange(turnsForSegment.TurnsB);
                }
            }

            return segments;
        }
    }

    internal class SegmentTurns
    {
        public string SegmentId { get; set; }
        public List<Turn> TurnsA { get; set; }
        public List<Turn> TurnsB { get; set; }
    }
}
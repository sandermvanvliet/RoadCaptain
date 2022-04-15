// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class SegmentStore : ISegmentStore
    {
        private readonly string _fileRoot;
        private readonly Dictionary<string, List<Segment>> _loadedSegments = new();

        public SegmentStore() : this(Environment.CurrentDirectory)
        {
        }

        internal SegmentStore(string fileRoot)
        {
            _fileRoot = fileRoot;
        }

        public List<Segment> LoadSegments(World world)
        {
            if (_loadedSegments.ContainsKey(world.Id))
            {
                return _loadedSegments[world.Id];
            }

            var segmentsPathForWorld = Path.Combine(_fileRoot, $"segments-{world.Id}.json");
            var turnsPathForWorld = Path.Combine(_fileRoot, $"turns-{world.Id}.json");

            if (!File.Exists(segmentsPathForWorld) || !File.Exists(turnsPathForWorld))
            {
                _loadedSegments.Add(world.Id, new List<Segment>());
                return _loadedSegments[world.Id];
            }

            var segments = JsonConvert.DeserializeObject<List<Segment>>(File.ReadAllText(segmentsPathForWorld));
            var turns = JsonConvert.DeserializeObject<List<SegmentTurns>>(File.ReadAllText(turnsPathForWorld));

            if (segments == null)
            {
                throw new Exception("Was unable to deserialize segments from file");
            }

            if (turns == null)
            {
                throw new Exception("Was unable to deserialize turns from file");
            }

            foreach (var segment in segments)
            {
                var turnsForSegment = turns.SingleOrDefault(t => t.SegmentId == segment.Id);

                if (turnsForSegment != null)
                {
                    if (turnsForSegment.TurnsA != null)
                    {
                        segment.NextSegmentsNodeA.AddRange(turnsForSegment.TurnsA.AsTurns());
                    }

                    if (turnsForSegment.TurnsB != null)
                    {
                        segment.NextSegmentsNodeB.AddRange(turnsForSegment.TurnsB.AsTurns());
                    }
                }
            }

            _loadedSegments.Add(world.Id, segments);

            return segments;
        }
    }

    internal class SegmentTurns
    {
        public string SegmentId { get; set; }
        public SegmentTurn TurnsA { get; set; }
        public SegmentTurn TurnsB { get; set; }
    }

    internal class SegmentTurn
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Left { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Right { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GoStraight { get; set; }

        public List<Turn> AsTurns()
        {
            List<Turn> turns = new();

            if (!string.IsNullOrEmpty(Left))
            {
                turns.Add(new Turn(TurnDirection.Left, Left));
            }
            
            if (!string.IsNullOrEmpty(Right))
            {
                turns.Add(new Turn(TurnDirection.Right, Right));
            }

            if (!string.IsNullOrEmpty(GoStraight))
            {
                turns.Add(new Turn(TurnDirection.GoStraight, GoStraight));
            }

            return turns;
        }
    }
}

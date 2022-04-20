// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class SegmentStore : ISegmentStore
    {
        private readonly string _fileRoot;
        private readonly Dictionary<string, List<Segment>> _loadedSegments = new();
        private readonly JsonSerializerSettings _serializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters =
            {
                new StringEnumConverter()
            }
        };

        public SegmentStore() : this(Environment.CurrentDirectory)
        {
        }

        internal SegmentStore(string fileRoot)
        {
            _fileRoot = fileRoot;
        }

        public List<Segment> LoadSegments(World world, SportType sport)
        {
            if (_loadedSegments.ContainsKey(CacheKey(sport, world)))
            {
                return _loadedSegments[CacheKey(sport, world)];
            }

            var segmentsPathForWorld = Path.Combine(_fileRoot, $"segments-{world.Id}.json");
            var turnsPathForWorld = Path.Combine(_fileRoot, $"turns-{world.Id}.json");

            if (!File.Exists(segmentsPathForWorld) || !File.Exists(turnsPathForWorld))
            {
                _loadedSegments.Add(CacheKey(sport, world), new List<Segment>());
                return _loadedSegments[CacheKey(sport, world)];
            }

            var segments = JsonConvert.DeserializeObject<List<Segment>>(File.ReadAllText(segmentsPathForWorld), _serializerSettings) ?? new List<Segment>();
            
            segments = segments
                .Where(segment => sport == SportType.Both || (segment.Sport == SportType.Both || segment.Sport == sport))
                .ToList();

            var turns = JsonConvert.DeserializeObject<List<SegmentTurns>>(File.ReadAllText(turnsPathForWorld), _serializerSettings);

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

            _loadedSegments.Add(CacheKey(sport, world), segments);

            return segments;
        }

        public List<Segment> LoadMarkers(World world)
        {
            if (_loadedSegments.ContainsKey(CacheKeyForMarkers(world)))
            {
                return _loadedSegments[CacheKeyForMarkers(world)];
            }

            var markersPathForWorld = Path.Combine(_fileRoot, $"markers-{world.Id}.json");

            if (!File.Exists(markersPathForWorld))
            {
                _loadedSegments.Add(CacheKeyForMarkers(world), new List<Segment>());
                return _loadedSegments[CacheKeyForMarkers(world)];
            }

            var markers = JsonConvert.DeserializeObject<List<Segment>>(File.ReadAllText(markersPathForWorld), _serializerSettings);
            
            if (markers == null)
            {
                throw new Exception("Was unable to deserialize markers from file");
            }

            _loadedSegments.Add(CacheKeyForMarkers(world), markers);

            return markers;
        }

        private string CacheKeyForMarkers(World world)
        {
            return $"markers-{world}";
        }

        private static string CacheKey(SportType sport, World world)
        {
            return $"{world.Id}-{sport}";
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

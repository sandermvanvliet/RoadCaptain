// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class SegmentStore : ISegmentStore
    {
        private readonly string _fileRoot;
        private static readonly ConcurrentDictionary<string, List<Segment>> LoadedSegments = new();
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
            if(LoadedSegments.TryGetValue(CacheKey(sport, world), out var cachedSegments))
            {
                return cachedSegments;
            }

            var binarySegmentsPathForWorld = Path.Combine(_fileRoot, $"segments-{world.Id}.bin");
            var segmentsPathForWorld = Path.Combine(_fileRoot, $"segments-{world.Id}.json");
            var turnsPathForWorld = Path.Combine(_fileRoot, $"turns-{world.Id}.json");

            var emptyListOfSegments = new List<Segment>();

            if ((!File.Exists(binarySegmentsPathForWorld) && !File.Exists(segmentsPathForWorld)) || !File.Exists(turnsPathForWorld))
            {
                LoadedSegments.TryAdd(CacheKey(sport, world), emptyListOfSegments);
                return emptyListOfSegments;
            }
            
            List<Segment> segments;

            if (File.Exists(binarySegmentsPathForWorld))
            {
                using var reader = new BinaryReader(File.OpenRead(binarySegmentsPathForWorld), Encoding.UTF8, false);
                segments = BinarySegmentSerializer.DeserializeSegments(reader);
            }
            else
            {
                segments = JsonConvert.DeserializeObject<List<Segment>>(File.ReadAllText(segmentsPathForWorld), _serializerSettings) ?? emptyListOfSegments;
            }

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

                foreach (var point in segment.Points)
                {
                    point.Segment = segment;
                }
            }

            LoadedSegments.TryAdd(CacheKey(sport, world), segments);

            return segments;
        }

        public List<Segment> LoadMarkers(World world)
        {
            if (LoadedSegments.ContainsKey(CacheKeyForMarkers(world)))
            {
                return LoadedSegments[CacheKeyForMarkers(world)];
            }

            var markersPathForWorld = Path.Combine(_fileRoot, $"markers-{world.Id}.json");

            if (!File.Exists(markersPathForWorld))
            {
                LoadedSegments.TryAdd(CacheKeyForMarkers(world), new List<Segment>());
                return LoadedSegments[CacheKeyForMarkers(world)];
            }

            var markers = JsonConvert.DeserializeObject<List<Segment>>(File.ReadAllText(markersPathForWorld), _serializerSettings);
            
            if (markers == null)
            {
                throw new Exception("Was unable to deserialize markers from file");
            }

            LoadedSegments.TryAdd(CacheKeyForMarkers(world), markers);

            return markers;
        }

        private static string CacheKeyForMarkers(World world)
        {
            return $"markers-{world.Id}";
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

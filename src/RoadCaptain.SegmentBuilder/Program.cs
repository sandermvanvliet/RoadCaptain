// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RoadCaptain.Adapters;

namespace RoadCaptain.SegmentBuilder
{
    class Program
    {
        // When splitting segments the overlap should be at least
        // this many meters along the segment to prevent the creation
        // of very short segments at three-way intersections for example.
        private const int MinimumDistanceAlongSegment = 100;

        static void Main(string[] args)
        {
            var gpxDirectory = args.Length > 0 ? args[0] : @"C:\git\temp\zwift\zwift-makuri_islands-gpx";

            new Program().Run(gpxDirectory);
        }

        private List<Segment> _segments = new();
        private readonly JsonSerializerSettings _serializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters =
            {
                new StringEnumConverter()
            }
        };

        public void Run(string gpxDirectory)
        {
            if (!File.Exists(Path.Combine(gpxDirectory, "segments", "snapshot-1.json")))
            {
                /*
                 * - Load the first route
                 * - Create a single segment from that route
                 * - Load the next route
                 * - Walk points and see if there is an existing segment that overlaps
                 *   - If so, ignore this point
                 *   - If not, start building a new segment
                 */
                var gpxFiles = Directory.GetFiles(gpxDirectory, "*.gpx");
                var toProcess = 2;
                foreach (var filePath in gpxFiles)
                {
                    var route = Route.FromGpxFile(Path.Combine(gpxDirectory, filePath));

                    Console.WriteLine($"Splitting {route.Slug} into segments");

                    var newSegments = route.SplitToSegments(_segments);

                    if (newSegments.Any())
                    {
                        Console.WriteLine($"Found {newSegments.Count} new segments");
                        _segments.AddRange(newSegments);
                    }

                    if (--toProcess <= 0)
                    {
                        break;
                    }
                }

                File.WriteAllText(Path.Combine(gpxDirectory, "segments", "snapshot-1.json"), JsonConvert.SerializeObject(_segments, _serializerSettings));
            }
            else
            {
                _segments = JsonConvert.DeserializeObject<List<Segment>>(File.ReadAllText(Path.Combine(gpxDirectory, "segments", "snapshot-1.json")), _serializerSettings);

                // Populate distance, index and parent properties
                // of track points on the segment.
                foreach (var segment in _segments)
                {
                    segment.CalculateDistances();
                }
            }

            /*
             * When we have a set of segments we can see where we have T-junctions,
             * A route start/end that is close to a point in another segment where
             * that point is somewhere in the middle of that segment.
             * For those matches we want to split up the larger segment.
             */
            var splitSteps = 1;

            while (splitSteps++ < 30)
            {
                Console.WriteLine($"\n========\nStarting segment split step: {splitSteps}\n");
                if (!SplitSegmentsAndUpdateSegmentList())
                {
                    break;
                }
            }

            GenerateTurns(_segments);

            var turns = _segments
                .Select(segment => new SegmentTurns
                {
                    SegmentId = segment.Id,
                    TurnsA = TurnsFromSegment(segment.NextSegmentsNodeA),
                    TurnsB = TurnsFromSegment(segment.NextSegmentsNodeB)
                })
                .ToList();

            foreach (var segment in _segments)
            {
                File.WriteAllText(Path.Combine(gpxDirectory, "segments", segment.Id + ".gpx"), segment.AsGpx());

                // Clear turns from segments otherwise it blows up because
                // loading the segments applies the turns to the segments
                segment.NextSegmentsNodeA.Clear();
                segment.NextSegmentsNodeB.Clear();
            }

            File.WriteAllText(
                Path.Combine(gpxDirectory, "segments", "segments.json"),
                JsonConvert.SerializeObject(_segments, Formatting.Indented, _serializerSettings));

            File.WriteAllText(
                Path.Combine(gpxDirectory, "segments", "turns.json"),
                JsonConvert.SerializeObject(turns, Formatting.Indented, _serializerSettings));
        }

        private static void GenerateTurns(List<Segment> segments)
        {
            foreach (var segment in segments)
            {
                FindOverlapsWithSegmentEnd(segments, segment, segment.A, segment.NextSegmentsNodeA);

                FindOverlapsWithSegmentEnd(segments, segment, segment.B, segment.NextSegmentsNodeB);
            }
        }

        private static void FindOverlapsWithSegmentEnd(List<Segment> segments, Segment segment, TrackPoint endPoint, List<Turn> endNode)
        {
            var overlaps = OverlapsWith(endPoint, segments, segment.Id);

            foreach (var overlap in overlaps)
            {
                var turnDirection = GetNextAvailableTurnDirection(endNode);
                if (endNode.All(n => n.SegmentId != overlap.Id))
                {
                    endNode.Add(new Turn(turnDirection, overlap.Id));
                }
            }
        }

        private static TurnDirection GetNextAvailableTurnDirection(List<Turn> turns)
        {
            var nextAvailable = new[] { TurnDirection.GoStraight, TurnDirection.Left, TurnDirection.Right }
                .Except(turns.Select(t => t.Direction).ToArray())
                .ToList()
                .FirstOrDefault();

            if (nextAvailable == default)
            {
                throw new InvalidOperationException("No turn direction available!");
            }

            return nextAvailable;
        }

        private static SegmentTurn TurnsFromSegment(List<Turn> turns)
        {
            var turn = new SegmentTurn();

            var left = turns.SingleOrDefault(t => t.Direction == TurnDirection.Left);
            if (left != null)
            {
                turn.Left = left.SegmentId;
            }
            var goStraight = turns.SingleOrDefault(t => t.Direction == TurnDirection.GoStraight);
            if (goStraight != null)
            {
                turn.GoStraight = goStraight.SegmentId;
            }
            var right = turns.SingleOrDefault(t => t.Direction == TurnDirection.Right);
            if (right != null)
            {
                turn.Right = right.SegmentId;
            }

            return turn;
        }

        public static List<Segment> OverlapsWith(TrackPoint point, List<Segment> segments, string currentSegmentId)
        {
            return segments
                .Where(s => s.Id != currentSegmentId)
                .Where(s => IsCloseTo(s.A, point) || IsCloseTo(s.B, point))
                .ToList();
        }

        public static bool IsCloseTo(TrackPoint point, TrackPoint other)
        {
            // 0.00013 degrees equivalent to 15 meters between degrees at latitude -11 
            // That means that if the difference in longitude between
            // the two points is more than 0.00013 then we're definitely
            // going to be more than 15 meters apart and that means
            // we're not close.
            if (Math.Abs(other.Longitude - point.Longitude) > 0.00013)
            {
                return false;
            }

            var distance = TrackPoint.GetDistanceFromLatLonInMeters(
                other.Latitude,
                other.Longitude,
                point.Latitude,
                point.Longitude);

            // TODO: re-enable altitude matching
            if (distance < 15 && Math.Abs(other.Altitude - point.Altitude) <= 2d)
            {
                return true;
            }

            return false;
        }

        private bool SplitSegmentsAndUpdateSegmentList()
        {
            var (toRemove, toAdd) = SplitSegmentsForOverlaps(_segments);

            foreach (var segment in toRemove)
            {
                if (_segments.Remove(segment))
                {
                    Console.WriteLine($"{segment.Id} was removed");
                }
                else
                {
                    Console.WriteLine($"{segment.Id} was NOT found!");
                }
            }

            foreach (var segment in toAdd)
            {
                Console.WriteLine($"{segment.Id} was added");
                _segments.Add(segment);
            }

            return toRemove.Any();
        }

        private (List<Segment> toRemove, List<Segment> toAdd) SplitSegmentsForOverlaps(List<Segment> segments)
        {
            var toRemove = new List<Segment>();
            var toAdd = new List<Segment>();

            foreach (var segment in segments)
            {
                var startOverlaps = FindOverlappingPointsInSegments(segment.A, _segments);

                foreach (var overlap in startOverlaps)
                {
                    if (overlap.DistanceOnSegment >= MinimumDistanceAlongSegment &&
                        overlap.Segment.B.DistanceOnSegment - overlap.DistanceOnSegment >= MinimumDistanceAlongSegment)
                    {
                        Console.WriteLine(
                            $"Found junction of start of {segment.Id} with {overlap.Segment.Id} {overlap.DistanceOnSegment:0}m along the segment");

                        var overlapIndex = overlap.Segment.Points.IndexOf(overlap);
                        if (overlapIndex <= 1)
                        {
                            Debugger.Break();
                        }

                        Console.WriteLine($"Splitting {overlap.Segment.Id} at index {overlapIndex}");

                        toRemove.Add(overlap.Segment);

                        var beforeSplit = overlap.Segment.Slice("before", 0, overlapIndex);
                        var afterSplit = overlap.Segment.Slice("after", overlapIndex);

                        toAdd.Add(beforeSplit);
                        toAdd.Add(afterSplit);

                        segment.NextSegmentsNodeA.Add(new Turn(TurnDirection.Left, beforeSplit.Id));
                        segment.NextSegmentsNodeA.Add(new Turn(TurnDirection.Right, afterSplit.Id));

                        beforeSplit.NextSegmentsNodeB.Add(new Turn(TurnDirection.GoStraight, afterSplit.Id));
                        beforeSplit.NextSegmentsNodeB.Add(new Turn(TurnDirection.Right, segment.Id));

                        afterSplit.NextSegmentsNodeA.Add(new Turn(TurnDirection.GoStraight, beforeSplit.Id));
                        afterSplit.NextSegmentsNodeA.Add(new Turn(TurnDirection.Left, segment.Id));
                    }
                }

                var endOverlaps = FindOverlappingPointsInSegments(segment.B, _segments);

                foreach (var overlap in endOverlaps)
                {
                    if (overlap.DistanceOnSegment >= MinimumDistanceAlongSegment &&
                        overlap.Segment.B.DistanceOnSegment - overlap.DistanceOnSegment >= MinimumDistanceAlongSegment)
                    {
                        Console.WriteLine(
                            $"Found junction of end of {segment.Id} with {overlap.Segment.Id} {overlap.DistanceOnSegment:0}m along the segment");

                        var overlapIndex = overlap.Segment.Points.IndexOf(overlap);
                        if (overlapIndex <= 1)
                        {
                            Debugger.Break();
                        }

                        Console.WriteLine($"Splitting {overlap.Segment.Id} at index {overlapIndex}");

                        toRemove.Add(overlap.Segment);

                        var beforeSplit = overlap.Segment.Slice("before", 0, overlapIndex);
                        var afterSplit = overlap.Segment.Slice("after", overlapIndex);

                        toAdd.Add(beforeSplit);
                        toAdd.Add(afterSplit);

                        segment.NextSegmentsNodeB.Add(new Turn(TurnDirection.Left, beforeSplit.Id));
                        segment.NextSegmentsNodeB.Add(new Turn(TurnDirection.Right, afterSplit.Id));

                        beforeSplit.NextSegmentsNodeB.Add(new Turn(TurnDirection.GoStraight, afterSplit.Id));
                        beforeSplit.NextSegmentsNodeB.Add(new Turn(TurnDirection.Right, segment.Id));

                        afterSplit.NextSegmentsNodeA.Add(new Turn(TurnDirection.GoStraight, beforeSplit.Id));
                        afterSplit.NextSegmentsNodeA.Add(new Turn(TurnDirection.Left, segment.Id));
                    }
                }

                if (toRemove.Any())
                {
                    // In situations where we have intersections on
                    // segments that we just added we need to ensure
                    // that we're not re-adding them later on. That
                    // will cause overlapping segments in the end result.
                    foreach (var segmentToRemove in toRemove)
                    {
                        if (toAdd.Any(t => t == segmentToRemove))
                        {
                            toAdd.Remove(segmentToRemove);
                        }
                    }

                    break;
                }
            }

            return (toRemove, toAdd);
        }

        private static List<TrackPoint> FindOverlappingPointsInSegments(TrackPoint point, List<Segment> segments)
        {
            return segments
                .AsParallel()
                .Select(segment => segment.Points.Where(p => IsCloseTo(p, point)))
                .Where(points => points.Any())
                .SelectMany(points => points)
                .ToList();
        }
    }
}


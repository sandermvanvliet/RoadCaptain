using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

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
            var gpxDirectory = args.Length > 0 ? args[0] : @"C:\git\temp\zwift\zwift-watopia-gpx";

            new Program().Run(gpxDirectory);
        }

        private List<Segment> _segments = new List<Segment>();

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
                }

                File.WriteAllText(Path.Combine(gpxDirectory, "segments", "snapshot-1.json"), JsonConvert.SerializeObject(_segments));
            }
            else
            {
                _segments = JsonConvert.DeserializeObject<List<Segment>>(File.ReadAllText(Path.Combine(gpxDirectory, "segments", "snapshot-1.json")));
            }

            // Poplulate distance, index and parent properties
            // of track points on the segment.
            foreach (var segment in _segments)
            {
                segment.CalculateDistances();
            }

            /*
             * When we have a set of segments we can see where we have T-junctions,
             * A route start/end that is close to a point in another segment where
             * that point is somehwere in the middle of that segment.
             * For those matches we want to split up the larger segment.
             */
            //var splitSteps = 1;

            //while (splitSteps++ < 30)
            //{
            //    Console.WriteLine($"\n========\nStarting segment split step: {splitSteps}\n");
            //    if (!SplitSegmentsAndUpdateSegmentList())
            //    {
            //        break;
            //    }
            //}

            foreach (var segment in _segments)
            {
                File.WriteAllText(Path.Combine(gpxDirectory, "segments", segment.Id + ".gpx"), segment.AsGpx());
            }
        }

        private bool SplitSegmentsAndUpdateSegmentList()
        {
            var result = SplitSegmentsForOverlaps(_segments);

            foreach (var segment in result.toRemove)
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

            foreach (var segment in result.toAdd)
            {
                Console.WriteLine($"{segment.Id} was added");
                _segments.Add(segment);
            }

            return result.toRemove.Any();
        }

        private (List<Segment> toRemove, List<Segment> toAdd) SplitSegmentsForOverlaps(List<Segment> segments)
        {
            var toRemove = new List<Segment>();
            var toAdd = new List<Segment>();

            foreach (var segment in segments)
            {
                var startOverlaps = FindOverlappingPointsInSegments(segment.Start, _segments);

                foreach (var overlap in startOverlaps)
                {
                    if (overlap.DistanceOnSegment >= MinimumDistanceAlongSegment &&
                        overlap.Segment.End.DistanceOnSegment - overlap.DistanceOnSegment >= MinimumDistanceAlongSegment)
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
                    }
                }

                var endOverlaps = FindOverlappingPointsInSegments(segment.End, _segments);

                foreach (var overlap in endOverlaps)
                {
                    if (overlap.DistanceOnSegment >= MinimumDistanceAlongSegment &&
                        overlap.Segment.End.DistanceOnSegment - overlap.DistanceOnSegment >= MinimumDistanceAlongSegment)
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
                    }
                }

                if (toRemove.Any())
                {
                    break;
                }
            }

            return (toRemove, toAdd);
        }

        private static List<TrackPoint> FindOverlappingPointsInSegments(TrackPoint point, List<Segment> segments)
        {
            return segments
                .AsParallel()
                .Select(segment => segment.Points.Where(p => p.IsCloseTo(point)))
                .Where(points => points.Any())
                .SelectMany(points => points)
                .ToList();
        }
    }
}

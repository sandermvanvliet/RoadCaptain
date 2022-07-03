using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RoadCaptain.SegmentBuilder
{
    internal class SegmentSplitStep
    {
        // When splitting segments the overlap should be at least
        // this many meters along the segment to prevent the creation
        // of very short segments at three-way intersections for example.
        private const int MinimumDistanceAlongSegment = 100;

        public static void Run(List<Segment> segments)
        {
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
                if (!SplitSegmentsAndUpdateSegmentList(segments))
                {
                    break;
                }
            }
        }

        private static bool SplitSegmentsAndUpdateSegmentList(List<Segment> segments)
        {
            var (toRemove, toAdd) = SplitSegmentsForOverlaps(segments);

            foreach (var segment in toRemove)
            {
                if (segments.Remove(segment))
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
                segments.Add(segment);
            }

            return toRemove.Any();
        }

        private static (List<Segment> toRemove, List<Segment> toAdd) SplitSegmentsForOverlaps(List<Segment> segments)
        {
            var toRemove = new List<Segment>();
            var toAdd = new List<Segment>();

            foreach (var segment in segments)
            {
                var startOverlaps = FindOverlappingPointsInSegments(segment.A, segments);

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

                var endOverlaps = FindOverlappingPointsInSegments(segment.B, segments);

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
                .Select(segment => segment.Points.Where(p => TrackPointUtils.IsCloseTo(p, point)))
                .Where(points => points.Any())
                .SelectMany(points => points)
                .ToList();
        }
    }
}
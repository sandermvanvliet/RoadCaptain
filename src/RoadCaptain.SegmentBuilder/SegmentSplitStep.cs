// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Serilog;

namespace RoadCaptain.SegmentBuilder
{
    internal class SegmentSplitStep : BaseStep
    {
        // When splitting segments the overlap should be at least
        // this many meters along the segment to prevent the creation
        // of very short segments at three-way intersections for example.
        private const int MinimumDistanceAlongSegment = 100;
        
        public override Context Run(Context context)
        {
            /*
             * When we have a set of segments we can see where we have T-junctions,
             * A route start/end that is close to a point in another segment where
             * that point is somewhere in the middle of that segment.
             * For those matches we want to split up the larger segment.
             */
            var splitSteps = 1;
            var segments = context.Segments.ToList();

            while (splitSteps++ < 30)
            {
                Logger.Information($"\n========\nStarting segment split step: {splitSteps}\n");
                if (!SplitSegmentsAndUpdateSegmentList(segments))
                {
                    break;
                }
            }

            return new Context(Step, segments, context.GpxDirectory, context.World);
        }

        private bool SplitSegmentsAndUpdateSegmentList(List<Segment> segments)
        {
            var (toRemove, toAdd) = SplitSegmentsForOverlaps(segments);

            foreach (var segment in toRemove)
            {
                if (segments.Remove(segment))
                {
                    Logger.Information($"{segment.Id} was removed");
                }
                else
                {
                    Logger.Information($"{segment.Id} was NOT found!");
                }
            }

            foreach (var segment in toAdd)
            {
                Logger.Information($"{segment.Id} was added");
                segments.Add(segment);
            }

            return toRemove.Any();
        }

        private (List<Segment> toRemove, List<Segment> toAdd) SplitSegmentsForOverlaps(List<Segment> segments)
        {
            var toRemove = new List<Segment>();
            var toAdd = new List<Segment>();

            foreach (var segment in segments)
            {
                var startOverlaps = FindOverlappingPointsInSegments(segment.A, segments);

                foreach (var overlap in startOverlaps)
                {
                    if (overlap.DistanceOnSegment >= MinimumDistanceAlongSegment &&
                        overlap.Segment!.B.DistanceOnSegment - overlap.DistanceOnSegment >= MinimumDistanceAlongSegment)
                    {
                        Logger.Information(
                            $"Found junction of start of {segment.Id} with {overlap.Segment.Id} {overlap.DistanceOnSegment:0}m along the segment");

                        var overlapIndex = overlap.Segment.Points.IndexOf(overlap);
                        if (overlapIndex <= 1)
                        {
                            Debugger.Break();
                        }

                        Logger.Information($"Splitting {overlap.Segment.Id} at index {overlapIndex}");

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
                        overlap.Segment!.B.DistanceOnSegment - overlap.DistanceOnSegment >= MinimumDistanceAlongSegment)
                    {
                        Logger.Information(
                            $"Found junction of end of {segment.Id} with {overlap.Segment.Id} {overlap.DistanceOnSegment:0}m along the segment");

                        var overlapIndex = overlap.Segment.Points.IndexOf(overlap);
                        if (overlapIndex <= 1)
                        {
                            Debugger.Break();
                        }

                        Logger.Information($"Splitting {overlap.Segment.Id} at index {overlapIndex}");

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

        public SegmentSplitStep(int step, ILogger logger) : base(logger, step)
        {
        }
    }
}

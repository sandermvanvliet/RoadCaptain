// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Serilog;

namespace RoadCaptain.SegmentBuilder
{
    internal class JunctionSplitterStep : BaseStep
    {
        public override Context Run(Context context)
        {
            var segments = context.Segments.ToList();

            while (true)
            {
                var breakWithChange = false;

                foreach (var segmentToAdjust in segments)
                {
                    var segmentsExceptSegmentToAdjust = segments
                        .Where(s => s.Id != segmentToAdjust.Id)
                        .ToList();

                    var (toRemove, toAdd) = SplitJunctionNode(
                        segmentToAdjust,
                        segmentsExceptSegmentToAdjust,
                        segmentToAdjust.A);

                    if (toRemove != null && toAdd != null)
                    {
                        segments.Remove(toRemove);
                        segments.AddRange(toAdd);
                        breakWithChange = true;
                        break;
                    }

                    (toRemove, toAdd) = SplitJunctionNode(
                        segmentToAdjust,
                        segmentsExceptSegmentToAdjust,
                        segmentToAdjust.B);

                    if (toRemove != null && toAdd != null)
                    {
                        segments.Remove(toRemove);
                        segments.AddRange(toAdd);
                        breakWithChange = true;
                        break;
                    }
                }

                if (breakWithChange)
                {
                    continue;
                }

                break;
            }

            return new Context(Step, segments, context.GpxDirectory, context.World);
        }

        private (Segment? toRemove, List<Segment>? toAdd) SplitJunctionNode(Segment segmentToAdjust, List<Segment> segmentsExceptSegmentToAdjust, TrackPoint endPoint)
        {
            Logger.Information($"Splitting segment {segmentToAdjust.Id} at node {endPoint.CoordinatesDecimal} of {segmentToAdjust.Id}");

            var overlaps = segmentsExceptSegmentToAdjust
                    .Select(segment => new
                    {
                        Segment = segment,
                        OverlappingPoints = segment
                            .Points
                            .Where(point => TrackPointUtils.IsCloseTo(point, endPoint, 25))
                            .ToList()
                    })
                    .Where(overlap => overlap.OverlappingPoints.Any())
                    .ToList();

            if (!overlaps.Any())
            {
                Logger.Information("Did not find overlaps");
                return (null, null);
            }

            if (overlaps.Count > 1)
            {
                Logger.Information("Found {Count} overlaps, attempting to adjust for T-junctions...", overlaps.Count);

                var beforeEndPoint = endPoint.Equals(segmentToAdjust.A)
                    ? segmentToAdjust.Points[1]
                    : segmentToAdjust.Points[^2];

                var segmentBearing = TrackPoint.Bearing(
                    endPoint,
                    beforeEndPoint);

                var temp = overlaps
                    .Where(overlap => overlap.OverlappingPoints.Count > 1)
                    .Select(overlap =>
                        new
                        {
                            Overlap = overlap,
                            OverlapBearing = TrackPoint.Bearing(
                                overlap.OverlappingPoints[0],
                                overlap.OverlappingPoints[^1])
                        })
                    .Select(x => new
                    {
                        x.Overlap,
                        x.OverlapBearing,
                        Difference = Math.Abs(segmentBearing - x.OverlapBearing)
                    })
                    .ToList();

                var newOverlaps = temp
                    .Where(x => x.Difference > 35)
                    .Select(x => x.Overlap)
                    .ToList();

                if (newOverlaps.Count == overlaps.Count || newOverlaps.Count > 1)
                {
                    Logger.Warning("Unable to adjust for T-junctions, too many overlaps to split a junction");
                    return (null, null);
                }

                if (newOverlaps.Count == 0)
                {
                    Logger.Warning("Unable to adjust for T-junction, got no overlaps left!");
                    return (null, null);
                }

                overlaps = newOverlaps;
                Logger.Information("Adjusted for a T-junction, got 1 segment left: {SegmentId}", overlaps[0].Segment.Id);
            }

            if (overlaps[0].OverlappingPoints.Count < 2)
            {
                Logger.Warning($"Found {overlaps[0].OverlappingPoints.Count} overlapping points but expected 2!");
                return (null, null);
            }

            var overlap = overlaps.Single();
            var junctionSegment = overlap.Segment;

            Logger.Information($"Found overlap with {junctionSegment.Id}");

            var byDistance = overlap
                .OverlappingPoints
                .Select(point => new
                {
                    Point = point,
                    Distance = TrackPoint.GetDistanceFromLatLonInMeters(point.Latitude, point.Longitude,
                        endPoint.Latitude, endPoint.Longitude)
                })
                .OrderBy(x => x.Distance)
                .ToList();

            var pointBefore = byDistance[0].Point;
            var pointAfter = byDistance[1].Point;

            if (pointBefore.Index > pointAfter.Index)
            {
                (pointAfter, pointBefore) = (pointBefore, pointAfter);
            }

            var minimumDistanceOnSegment = 100;

            if (pointAfter.DistanceOnSegment < minimumDistanceOnSegment || pointAfter.DistanceOnSegment > junctionSegment.Distance - minimumDistanceOnSegment)
            {
                Logger.Warning("Overlap point is {Distance}m on segment but expected at least 100m from start or end of the segment", Math.Round(pointAfter.DistanceOnSegment, 1));
                return (null, null);
            }

            var before = junctionSegment.Slice("before", 0, pointAfter.Index!.Value);
            var after = junctionSegment.Slice("after", pointAfter.Index.Value + 1);

            return (
                junctionSegment, 
                new List<Segment> { before, after });
        }

        public JunctionSplitterStep(int step, ILogger logger) : base(logger, step)
        {
        }
    }
}

// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;

namespace RoadCaptain.SegmentBuilder
{
    internal class JunctionSplitterStep
    {
        public static void Run(List<Segment> segments)
        {
            var haveChanges = true;

            while (haveChanges)
            {
                var breakWithChange = false;

                foreach (var segmentToAdjust in segments)
                {
                    var (toRemove, toAdd) = SplitJunctionNode(
                        segmentToAdjust,
                        segments
                            .Where(s => s.Id != segmentToAdjust.Id)
                            .ToList(),
                        segmentToAdjust.A);

                    if (toRemove != null)
                    {
                        segments.Remove(toRemove);
                        segments.AddRange(toAdd);
                        breakWithChange = true;
                        break;
                    }

                    (toRemove, toAdd) = SplitJunctionNode(
                        segmentToAdjust,
                        segments
                            .Where(s => s.Id != segmentToAdjust.Id)
                            .ToList(),
                        segmentToAdjust.B);

                    if (toRemove != null)
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
        }

        private static (Segment toRemove, List<Segment> toAdd) SplitJunctionNode(Segment segmentToAdjust, List<Segment> segmentsExceptSegmentToAdjust, TrackPoint endPoint)
        {
            Console.WriteLine($"Splitting segment {segmentToAdjust.Id} at node {endPoint.CoordinatesDecimal} of {segmentToAdjust.Id}");

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
                Console.WriteLine("\tDid not find overlaps!");
                return (null, null);
            }

            if (overlaps.Count > 1)
            {
                Console.WriteLine($"\tFound {overlaps.Count} overlaps but only expected 1!");
                return (null, null);
            }

            if (overlaps[0].OverlappingPoints.Count < 2)
            {
                Console.WriteLine($"\tFound {overlaps[0].OverlappingPoints.Count} overlapping points but expected 2!");
                return (null, null);
            }

            var overlap = overlaps.Single();
            var junctionSegment = overlap.Segment;

            Console.WriteLine($"\tFound overlap with {junctionSegment.Id}");

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

            if (pointAfter.DistanceOnSegment < 100 || pointAfter.DistanceOnSegment > junctionSegment.Distance - 100)
            {
                Console.WriteLine($"\tOverlap point is {pointAfter.DistanceOnSegment}m on segment but expected at least 100m from start or end of the segment");
                return (null, null);
            }

            var before = junctionSegment.Slice("before", 0, pointAfter.Index.Value);
            var after = junctionSegment.Slice("after", pointAfter.Index.Value + 1);

            return (
                junctionSegment, 
                new List<Segment> { before, after });
        }
    }
}

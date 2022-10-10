// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;

namespace RoadCaptain.SegmentBuilder
{
    internal class SegmentSmootherStep
    {
        public static void Run(List<Segment> segments)
        {
            // The original GPX files have track points that are the same location.
            // Because later on we always expect that there is a distance between
            // two track points we need to ensure that there are no zero-distance
            // points in a segment.
            // This step takes care of cleaning up those points from a segment and
            // recalculates distances and indexes accordingly.

            foreach (var segment in segments)
            {
                SmoothSegment(segment);
            }
        }

        private static void SmoothSegment(Segment segment)
        {
            Console.WriteLine($"Smoothing segment {segment.Id}");

            var smoothedPoints = new List<TrackPoint>();
            TrackPoint previous = null;
            var numberOfPointsSkipped = 0;

            foreach (var point in segment.Points)
            {
                if (previous == null)
                {
                    previous = point;
                    continue;
                }

                if (!point.Equals(previous))
                {
                    smoothedPoints.Add(point);
                    previous = point;
                }
                else
                {
                    Console.WriteLine($"\tSkipping {point.CoordinatesDecimal}");
                    numberOfPointsSkipped++;
                }
            }

            if (numberOfPointsSkipped > 0)
            {
                Console.WriteLine($"\tSkipped {numberOfPointsSkipped} points from this segment");
            }

            segment.Points.Clear();
            segment.Points.AddRange(smoothedPoints);
            segment.CalculateDistances();
        }
    }
}

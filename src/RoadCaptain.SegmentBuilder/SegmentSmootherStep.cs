// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace RoadCaptain.SegmentBuilder
{
    internal class SegmentSmootherStep : BaseStep
    {
        public override Context Run(Context context)
        {
            // The original GPX files have track points that are the same location.
            // Because later on we always expect that there is a distance between
            // two track points we need to ensure that there are no zero-distance
            // points in a segment.
            // This step takes care of cleaning up those points from a segment and
            // recalculates distances and indexes accordingly.

            var smoothedSegments = context
                .Segments
                .Select(SmoothSegment)
                .ToList();

            return new Context(Step, smoothedSegments, context.GpxDirectory, context.World);
        }

        private Segment SmoothSegment(Segment segment)
        {
            Logger.Information("Smoothing segment {SegmentId}", segment.Id);

            var smoothedPoints = new List<TrackPoint>();
            TrackPoint? previous = null;
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
                    Logger.Information("Skipping {CoordinatesDecimal}", point.CoordinatesDecimal);
                    numberOfPointsSkipped++;
                }
            }

            if (numberOfPointsSkipped > 0)
            {
                Logger.Information("Skipped {NumberOfPointsSkipped} points from this segment", numberOfPointsSkipped);
            }

            var smoothedSegment = new Segment(smoothedPoints)
            {
                Id = segment.Id,
                Name = segment.Name,
                NoSelectReason = segment.NoSelectReason,
                Sport = segment.Sport,
                Type = segment.Type
            };
            smoothedSegment.CalculateDistances();

            return smoothedSegment;
        }

        public SegmentSmootherStep(int step, ILogger logger) : base(logger, step)
        {
        }
    }
}

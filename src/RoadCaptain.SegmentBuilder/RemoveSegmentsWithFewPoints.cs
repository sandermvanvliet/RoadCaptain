// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using Serilog;

namespace RoadCaptain.SegmentBuilder
{
    internal class RemoveSegmentsWithFewPoints : BaseStep
    {
        public RemoveSegmentsWithFewPoints(int step, ILogger logger) : base(logger, step)
        {
        }

        public override Context Run(Context context)
        {
            var segments = new List<Segment>();

            foreach (var segment in context.Segments)
            {
                if (segment.Points.Count <= 3)
                {
                    Logger.Warning("Segment {SegmentId} only has {Count} points and is too short", segment.Id, segment.Points.Count);
                }
                else
                {
                    segments.Add(segment);
                }
            }

            return new Context(Step, segments, context.GpxDirectory, context.World);
        }
    }
}

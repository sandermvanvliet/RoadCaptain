// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace RoadCaptain.SegmentBuilder
{
    internal class RemoveSegmentsShorterThan20Meters : BaseStep
    {
        public RemoveSegmentsShorterThan20Meters(int step, ILogger logger) : base(logger, step)
        {
        }

        public override Context Run(Context context)
        {
            var segments = new List<Segment>();

            foreach (var segment in context.Segments)
            {
                if (segment.Distance < 20)
                {
                    Logger.Warning("Segment {SegmentId} is only {Distance}m long and is too short", segment.Id, segment.Distance);
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

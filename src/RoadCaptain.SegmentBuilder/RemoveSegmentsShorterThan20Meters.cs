// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
            // Remove very short segments
            return new Context(Step, context
                    .Segments
                    .Where(segment => segment.Distance >= 20)
                    .ToList(),
                context.GpxDirectory);
        }
    }
}

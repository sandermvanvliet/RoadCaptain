// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Linq;
using Serilog;

namespace RoadCaptain.SegmentBuilder
{
    internal class JoinStraightConnections : BaseStep
    {
        public JoinStraightConnections(int step, ILogger logger) : base(logger, step)
        {
        }

        public override Context Run(Context context)
        {
            var segmentsWithOnlyGoStraight = context
                .Segments
                .Where(segment => segment.NextSegmentsNodeA.Count == 1)
                .ToList();

            foreach (var segment in segmentsWithOnlyGoStraight)
            {
                
            }
            
            return new Context(Step, context.Segments.ToList(), context.GpxDirectory, context.World);
        }
    }
}

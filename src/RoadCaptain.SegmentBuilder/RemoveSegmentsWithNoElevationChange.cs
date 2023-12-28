using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace RoadCaptain.SegmentBuilder
{
    internal class RemoveSegmentsWithNoElevationChange : BaseStep
    {
        public RemoveSegmentsWithNoElevationChange(int step, ILogger logger) : base(logger, step)
        {
        }

        public override Context Run(Context context)
        {
            // Remove segments that have no ascent or descent...
            // The super flat ones which apparently don't have 
            // any elevation data.
            var segments = new List<Segment>();

            foreach (var segment in context.Segments)
            {
                if (segment is { Ascent: 0, Descent: 0 })
                {
                    Logger.Warning("Segment {SegmentId} does not have any elevation change", segment.Id);
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
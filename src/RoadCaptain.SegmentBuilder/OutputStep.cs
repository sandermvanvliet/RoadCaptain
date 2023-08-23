// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Serilog;

namespace RoadCaptain.SegmentBuilder
{
    internal class OutputStep : BaseStep
    {
        public override Context Run(Context context)
        {
            if (!Directory.Exists(Path.Combine(context.GpxDirectory, "segments")))
            {
                Directory.CreateDirectory(Path.Combine(context.GpxDirectory, "segments"));
            }

            var segments = context.Segments.ToList();

            foreach (var segment in segments)
            {
                File.WriteAllText(Path.Combine(context.GpxDirectory, "segments", segment.Id + ".gpx"), segment.AsGpx());

                // Clear turns from segments otherwise it blows up because
                // loading the segments applies the turns to the segments
                segment.NextSegmentsNodeA.Clear();
                segment.NextSegmentsNodeB.Clear();
            }

            File.WriteAllText(
                Path.Combine(context.GpxDirectory, "segments", "segments.json"),
                JsonConvert.SerializeObject(segments.OrderBy(s=>s.Id).ToList(), Formatting.Indented, Program.SerializerSettings));

            return new Context(Step, segments, context.GpxDirectory);
        }

        public OutputStep(int step, ILogger logger) : base(logger, step)
        {
        }
    }
}

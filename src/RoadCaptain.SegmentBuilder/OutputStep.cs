// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Serilog;

namespace RoadCaptain.SegmentBuilder
{
    internal class OutputStep : Step
    {
        public override Context Run(Context context)
        {
            if (!Directory.Exists(Path.Combine(context.GpxDirectory, "segments")))
            {
                Directory.CreateDirectory(Path.Combine(context.GpxDirectory, "segments"));
            }

            foreach (var segment in context.Segments)
            {
                File.WriteAllText(Path.Combine(context.GpxDirectory, "segments", segment.Id + ".gpx"), segment.AsGpx());

                // Clear turns from segments otherwise it blows up because
                // loading the segments applies the turns to the segments
                segment.NextSegmentsNodeA.Clear();
                segment.NextSegmentsNodeB.Clear();
            }

            File.WriteAllText(
                Path.Combine(context.GpxDirectory, "segments", "segments.json"),
                JsonConvert.SerializeObject(context.Segments.OrderBy(s=>s.Id).ToList(), Formatting.Indented, Program.SerializerSettings));

            return context;
        }

        public OutputStep(ILogger logger) : base(logger)
        {
        }
    }
}

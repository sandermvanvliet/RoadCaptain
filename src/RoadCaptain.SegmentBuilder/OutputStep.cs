// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.SegmentBuilder
{
    internal class OutputStep
    {
        public static void Run(List<Segment> segments, string gpxDirectory)
        {
            if (!Directory.Exists(Path.Combine(gpxDirectory, "segments")))
            {
                Directory.CreateDirectory(Path.Combine(gpxDirectory, "segments"));
            }

            foreach (var segment in segments)
            {
                File.WriteAllText(Path.Combine(gpxDirectory, "segments", segment.Id + ".gpx"), segment.AsGpx());

                // Clear turns from segments otherwise it blows up because
                // loading the segments applies the turns to the segments
                segment.NextSegmentsNodeA.Clear();
                segment.NextSegmentsNodeB.Clear();
            }

            File.WriteAllText(
                Path.Combine(gpxDirectory, "segments", "segments.json"),
                JsonConvert.SerializeObject(segments.OrderBy(s=>s.Id).ToList(), Formatting.Indented, Program.SerializerSettings));
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.SegmentBuilder
{
    internal class GpxToSegmentsStep
    {
        public static void Run(List<Segment> segments, string gpxDirectory)
        {
            /*
                 * - Load the first route
                 * - Create a single segment from that route
                 * - Load the next route
                 * - Walk points and see if there is an existing segment that overlaps
                 *   - If so, ignore this point
                 *   - If not, start building a new segment
                 */
            var gpxFiles = Directory.GetFiles(gpxDirectory, "*.gpx");
            foreach (var filePath in gpxFiles)
            {
                var route = Route.FromGpxFile(Path.Combine(gpxDirectory, filePath));

                Console.WriteLine($"Splitting {route.Slug} into segments");

                var newSegments = route.SplitToSegments(segments);

                if (newSegments.Any())
                {
                    Console.WriteLine($"Found {newSegments.Count} new segments");
                    segments.AddRange(newSegments);
                }
            }

            File.WriteAllText(Path.Combine(gpxDirectory, "segments", "snapshot-1.json"), JsonConvert.SerializeObject(segments, Program.SerializerSettings));
        }
    }
}
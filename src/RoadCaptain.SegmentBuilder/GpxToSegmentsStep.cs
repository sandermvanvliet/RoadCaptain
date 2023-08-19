// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Serilog;

namespace RoadCaptain.SegmentBuilder
{
    internal class GpxToSegmentsStep : Step
    {
        public override Context Run(Context context)
        {
            var snapShotPath = Path.Combine(context.GpxDirectory, "segments", "snapshot-1.json");

            if (File.Exists(snapShotPath))
            {
                var snapshotSegments = JsonConvert.DeserializeObject<List<Segment>>(File.ReadAllText(snapShotPath), Program.SerializerSettings);

                return new Context(snapshotSegments!, context.GpxDirectory);
            }

            if(!Directory.Exists(Path.Combine(context.GpxDirectory, "segments")))
            {
                Directory.CreateDirectory(Path.Combine(context.GpxDirectory, "segments"));
            }

            /*
                 * - Load the first route
                 * - Create a single segment from that route
                 * - Load the next route
                 * - Walk points and see if there is an existing segment that overlaps
                 *   - If so, ignore this point
                 *   - If not, start building a new segment
                 */
            var segments = new List<Segment>();
            var gpxFiles = Directory.GetFiles(context.GpxDirectory, "*.gpx");
            foreach (var filePath in gpxFiles)
            {
                var route = Route.FromGpxFile(Path.Combine(context.GpxDirectory, filePath));

                // A Makuri Islands special fix, for some reason a number of segments
                // in the Neyoko area have a different base altitude which causes the
                // route overlap matching to go completely off the rails.
                // This corrects for that by detecting segments for which all track points
                // are above 120m and then subtracting 60m from all of them to get them
                // back into alignment.
                if (route.TrackPoints.Min(p => p.Altitude) >= 120)
                {
                    route.TrackPoints = route
                        .TrackPoints
                        .Select(trackPoint => new TrackPoint(trackPoint.Latitude, trackPoint.Longitude, trackPoint.Altitude - 60, trackPoint.WorldId))
                        .ToList();
                }

                Logger.Information($"Splitting {route.Slug} into segments");

                var newSegments = route.SplitToSegments(segments);

                if (newSegments.Any())
                {
                    Logger.Information($"Found {newSegments.Count} new segments");
                    segments.AddRange(newSegments);
                }
            }

            File.WriteAllText(snapShotPath, JsonConvert.SerializeObject(segments, Program.SerializerSettings));

            return new Context(segments, context.GpxDirectory);
        }

        public GpxToSegmentsStep(ILogger logger) : base(logger)
        {
        }
    }
}

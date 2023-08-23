// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Serilog;

namespace RoadCaptain.SegmentBuilder
{
    internal class SpawnPointFinderStep : BaseStep
    {
        public override Context Run(Context context)
        {
            var spawnPoints = new List<SpawnPoint>();

            var gpxFiles = Directory.GetFiles(context.GpxDirectory, "*.gpx");
            foreach (var filePath in gpxFiles)
            {
                var route = Route.FromGpxFile(Path.Combine(context.GpxDirectory, filePath));

                Logger.Information($"Finding spawn point segment for {route.Slug}");

                // To make sure that we don't have the route matching
                // at a right angle (for example) on another segment
                // we take two points along the route.
                var firstTrackPoint = route.TrackPoints[10];
                var secondTrackPoint = route.TrackPoints[20];

                var segmentsCloseBy = context.Segments
                    .Select(s =>
                    {
                        var containsFirstTrackPoint = s.Contains(firstTrackPoint, out var firstMatch);
                        var containsSecondTrackPoint = s.Contains(secondTrackPoint, out var secondMatch);

                        var contains = containsFirstTrackPoint && containsSecondTrackPoint;

                        return new
                        {
                            ContainsTrackPoints = contains,
                            Segment = contains ? s : null,
                            FirstTrackPointOnSegment = firstMatch,
                            SecondTrackPointOnSegment =  secondMatch,
                            // Only do distance to first one since we're only using it for ordering...
                            Distance = contains ? firstMatch!.DistanceTo(firstTrackPoint) : (double?)null
                        };
                    })
                    .Where(x => x.ContainsTrackPoints)
                    .ToList();

                var firstMatch = segmentsCloseBy.FirstOrDefault();

                if (firstMatch == null)
                {
                    Logger.Information($"Did not find point {firstTrackPoint.CoordinatesDecimal} on any segment");
                    continue;
                }

                Segment segment;

                if (segmentsCloseBy.Count > 1)
                {
                    firstMatch = segmentsCloseBy.MinBy(x => x.Distance);
                    segment = firstMatch!.Segment!;
                }
                else
                {
                    segment = firstMatch.Segment!;
                }
                
                Logger.Information($"Found point {firstTrackPoint.CoordinatesDecimal} on segment {segment.Id} as {firstMatch.FirstTrackPointOnSegment!.CoordinatesDecimal}");

                if (spawnPoints.Any(s => s.SegmentId == segment.Id))
                {
                    Logger.Warning($"{segment.Id} is already a spawn point");
                    continue;
                }

                var direction = segment.DirectionOf(firstMatch.FirstTrackPointOnSegment, firstMatch.SecondTrackPointOnSegment!);

                Logger.Information($"Adding spawn point for {route.Name} with direction {direction}");

                spawnPoints.Add(new SpawnPoint
                {
                    SegmentId = segment.Id,
                    Direction = direction,
                    Sport = segment.Sport,
                    ZwiftRouteName = route.Name
                });
            }

            File.WriteAllText(
                Path.Combine(context.GpxDirectory, "segments", "spawnPoints.json"),
                JsonConvert.SerializeObject(spawnPoints.OrderBy(s => s.SegmentId).ToList(), Formatting.Indented, Program.SerializerSettings));

            return new Context(Step, context.Segments.ToList(), context.GpxDirectory);
        }

        public SpawnPointFinderStep(int step, ILogger logger) : base(logger, step)
        {
        }
    }
}

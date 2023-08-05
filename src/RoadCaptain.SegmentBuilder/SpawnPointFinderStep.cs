// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.SegmentBuilder
{
    internal class SpawnPointFinderStep
    {
        public static void Run(List<Segment> segments, string gpxDirectory)
        {
            var spawnPoints = new List<SpawnPoint>();

            var gpxFiles = Directory.GetFiles(gpxDirectory, "*.gpx");
            foreach (var filePath in gpxFiles)
            {
                var route = Route.FromGpxFile(Path.Combine(gpxDirectory, filePath));

                Console.WriteLine($"Finding spawn point segment for {route.Slug}");

                // To make sure that we don't have the route matching
                // at a right angle (for example) on another segment
                // we take two points along the route.
                var firstTrackPoint = route.TrackPoints[10];
                var secondTrackPoint = route.TrackPoints[20];

                var segmentsCloseBy = segments
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
                    Console.WriteLine($"\tDid not find point {firstTrackPoint.CoordinatesDecimal} on any segment");
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
                
                Console.WriteLine($"\tFound point {firstTrackPoint.CoordinatesDecimal} on segment {segment.Id} as {firstMatch.FirstTrackPointOnSegment!.CoordinatesDecimal}");

                if (spawnPoints.Any(s => s.SegmentId == segment.Id))
                {
                    Console.WriteLine($"\t{segment.Id} is already a spawn point");
                    continue;
                }

                var direction = segment.DirectionOf(firstMatch.FirstTrackPointOnSegment, firstMatch.SecondTrackPointOnSegment!);

                Console.WriteLine($"\tAdding spawn point for {route.Name} with direction {direction}");

                spawnPoints.Add(new SpawnPoint
                {
                    SegmentId = segment.Id,
                    Direction = direction,
                    Sport = segment.Sport,
                    ZwiftRouteName = route.Name
                });
            }

            File.WriteAllText(
                Path.Combine(gpxDirectory, "segments", "spawnPoints.json"),
                JsonConvert.SerializeObject(spawnPoints.OrderBy(s => s.SegmentId).ToList(), Formatting.Indented, Program.SerializerSettings));
        }
    }
}

// Copyright (c) 2022 Sander van Vliet
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

                var trackPoint = route.TrackPoints[10];

                var segment = segments.SingleOrDefault(s => s.Points.Any(p => TrackPointUtils.IsCloseTo(p, trackPoint)));

                if (segment == null)
                {
                    Console.WriteLine($"\tDid not find point {trackPoint.CoordinatesDecimal} on any segment");
                    continue;
                }

                trackPoint = GetClosestPointOnSegment(segment, trackPoint);
                var nextTrackPoint = GetClosestPointOnSegment(segment, route.TrackPoints[20]);

                Console.WriteLine($"\tFound point {trackPoint.CoordinatesDecimal} on segment {segment.Id}");

                if (spawnPoints.Any(s => s.SegmentId == segment.Id))
                {
                    Console.WriteLine($"\t{segment.Id} is already a spawn point");
                    continue;
                }
                
                var direction = segment.DirectionOf(trackPoint, nextTrackPoint);

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
                JsonConvert.SerializeObject(spawnPoints.OrderBy(s=>s.SegmentId).ToList(), Formatting.Indented, Program.SerializerSettings));
        }

        private static TrackPoint GetClosestPointOnSegment(Segment segment, TrackPoint trackPoint)
        {
            return segment
                .Points
                .Where(p => TrackPointUtils.IsCloseTo(p, trackPoint))
                .Select(p => new
                {
                    Point = p,
                    Distance = TrackPoint.GetDistanceFromLatLonInMeters(p.Latitude, p.Longitude,
                        trackPoint.Latitude, trackPoint.Longitude)
                })
                .OrderBy(p => p.Distance)
                .First()
                .Point;
        }
    }
}

// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

                Logger.Information("Finding spawn point segment for {RouteSlug}", route.Slug);

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
                    Logger.Information("Did not find point {CoordinatesDecimal} on any segment", firstTrackPoint.CoordinatesDecimal);
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
                
                Logger.Information("Found point {CoordinatesDecimal} on segment {SegmentId} as {CoordinatesOnSegment}", firstTrackPoint.CoordinatesDecimal, segment.Id, firstMatch.FirstTrackPointOnSegment!.CoordinatesDecimal);


                var direction = segment.DirectionOf(firstMatch.FirstTrackPointOnSegment, firstMatch.SecondTrackPointOnSegment!);

                if (spawnPoints.Any(s => s.SegmentId == segment.Id && s.Direction == direction))
                {
                    Logger.Warning("{SegmentId} is already a spawn point for the direction {Direction}", segment.Id, direction);
                    continue;
                }
                
                Logger.Information("Adding spawn point for {RouteName} with direction {Direction}", route.Name, direction);

                spawnPoints.Add(new SpawnPoint
                {
                    SegmentId = segment.Id,
                    Direction = direction,
                    Sport = segment.Sport,
                    ZwiftRouteName = route.Name
                });
            }

            var zwiftWorldId = Enum.Parse<ZwiftWorldId>(context.World, true);
            var (worldMostLeft, worldMostRight) = CalculateLeftRight(context.Segments, zwiftWorldId);

            File.WriteAllText(
                Path.Combine(context.GpxDirectory, "segments", $"spawnPoints-{context.World}.json"),
                JsonConvert.SerializeObject(
                    new
                    {
                        id = context.World,
                        worldMostLeft,
                        worldMostRight,
                        spawnPoints = spawnPoints.OrderBy(s => s.SegmentId).ToList()
                    },
                    Formatting.Indented,
                    Program.SerializerSettings));

            return new Context(Step, context.Segments.ToList(), context.GpxDirectory, context.World);
        }

        private (TrackPoint worldMostLeft, TrackPoint worldMostRight) CalculateLeftRight(
            ImmutableList<Segment> segments, ZwiftWorldId zwiftWorldId)
        {
            var allPoints = segments.SelectMany(s => s.Points).ToList();

            double? mapLeftLat = null;
            double? mapLeftLon = null;
            double? mapRightLat = null;
            double? mapRightLon = null;
            
            foreach (var point in allPoints)
            {
                mapLeftLat = mapLeftLat.HasValue
                    ? point.Latitude > mapLeftLat
                        ? point.Latitude
                        : mapLeftLat
                    : point.Latitude;
                
                mapLeftLon = mapLeftLon.HasValue
                    ? point.Longitude < mapLeftLon
                        ? point.Longitude
                        : mapLeftLon
                    : point.Longitude;
                
                mapRightLat = mapRightLat.HasValue
                    ? point.Latitude < mapRightLat
                        ? point.Latitude
                        : mapRightLat
                    : point.Latitude;
                
                mapRightLon = mapRightLon.HasValue
                    ? point.Longitude > mapRightLon
                        ? point.Longitude
                        : mapRightLon
                    : point.Longitude;
            }

            return (new TrackPoint(mapLeftLat!.Value, mapLeftLon!.Value, 0, zwiftWorldId),
                new TrackPoint(mapRightLat!.Value, mapRightLon!.Value, 0, zwiftWorldId));
        }

        public SpawnPointFinderStep(int step, ILogger logger) : base(logger, step)
        {
        }
    }
}

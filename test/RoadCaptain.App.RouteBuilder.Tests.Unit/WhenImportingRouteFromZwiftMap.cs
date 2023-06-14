using System;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using RoadCaptain.Adapters;
using Xunit;
using RoadCaptain.App.RouteBuilder.Models;

namespace RoadCaptain.App.RouteBuilder.Tests.Unit
{
    public class WhenImportingRouteFromZwiftMap
    {
        [Fact]
        public void GivenZwiftMapRoute_ItIsMappedToAPlannedRoute()
        {
            var fileRoot = Environment.CurrentDirectory;
            var segmentStore = new SegmentStore(fileRoot);
            var worldStore = new WorldStoreToDisk(fileRoot);
            var routeStore = new RouteStoreToDisk(segmentStore, worldStore);

            var expectedPlannedRoute = routeStore.LoadFrom("ImportedFromZwiftMap.json");

            var zwiftMapRoute = ZwiftMapRoute.FromGpxFile("zwiftmap-route.gpx");
            
            var worldName = "watopia";
            
            if (!string.IsNullOrEmpty(zwiftMapRoute.Link))
            {
                worldName = zwiftMapRoute.Link.Split("/")[3];
            }

            var world = worldStore.LoadWorldById(worldName);

            if (world == null)
            {
                throw new Exception($"Unable to find world '{world}'");
            }

            var result = new PlannedRoute
            {
                Name = zwiftMapRoute.Name,
                World = world,
                Sport = SportType.Cycling
            };
            
            var segments = segmentStore.LoadSegments(world!, SportType.Cycling);

            Segment? previousSegment = null;
            Segment? currentSegment = null;
            TrackPoint? firstMatchOnSegment = null;
            TrackPoint? lastMatchOnSegment = null;
            Segment[]? nextSegmentOptions = null;

            foreach (var trackPoint in zwiftMapRoute.TrackPoints)
            {
                if (currentSegment == null && result.RouteSegmentSequence.Count == 0)
                {
                    // Expensive first lookup
                    var matchingSegments = segments.Where(segment => segment.Contains(trackPoint)).ToList();

                    if (matchingSegments.Count == 0)
                    {
                        throw new Exception(
                            "Unable to locate the first track point of the route, can't convert to a RoadCaptain route");
                    }

                    if (matchingSegments.Count > 1)
                    {
                        continue;
                    }

                    currentSegment = matchingSegments.First();
                    currentSegment.Contains(trackPoint, out var matchingTrackPoint);
                    firstMatchOnSegment = matchingTrackPoint;
                    lastMatchOnSegment = matchingTrackPoint;
                }
                else if (currentSegment != null)
                {
                    var currentSegmentContainsTrackPoint = currentSegment.Contains(trackPoint, out var matchingTrackPoint);
                    if (!currentSegmentContainsTrackPoint)
                    {
                        // Entering next segment
                        var direction = currentSegment.DirectionOf(firstMatchOnSegment, lastMatchOnSegment);

                        result.RouteSegmentSequence.Add(new SegmentSequence(currentSegment.Id, direction));

                        if (result.RouteSegmentSequence.Count > 1)
                        {
                            result.RouteSegmentSequence[^2].NextSegmentId = currentSegment.Id;
                            result.RouteSegmentSequence[^2].TurnToNextSegment =
                                result.RouteSegmentSequence[^2].Direction == SegmentDirection.AtoB
                                    ? previousSegment.NextSegmentsNodeB.Single(n => n.SegmentId == currentSegment.Id)
                                        .Direction
                                    : previousSegment.NextSegmentsNodeA.Single(n => n.SegmentId == currentSegment.Id)
                                        .Direction;
                        }

                        string[] nextSegmentIds;

                        if (direction == SegmentDirection.AtoB)
                        {
                            nextSegmentIds = currentSegment.NextSegmentsNodeB.Select(t => t.SegmentId).ToArray();
                        }
                        else if (direction == SegmentDirection.BtoA)
                        {
                            nextSegmentIds = currentSegment.NextSegmentsNodeA.Select(t => t.SegmentId).ToArray();
                        }
                        else
                        {
                            throw new Exception("BANG!");
                        }

                        nextSegmentOptions = segments.Where(s => nextSegmentIds.Contains(s.Id)).ToArray();

                        if (nextSegmentOptions.Length > 1)
                        {
                            if (Debugger.IsAttached)
                            {
                                Debugger.Break();
                            }
                        }

                        previousSegment = currentSegment;
                        currentSegment = null;
                        firstMatchOnSegment = null;
                        lastMatchOnSegment = null;
                    }
                    else if (currentSegmentContainsTrackPoint)
                    {
                        lastMatchOnSegment = matchingTrackPoint;
                        continue;
                    }
                }

                if (currentSegment == null && nextSegmentOptions != null)
                {
                    var matchingSegments = nextSegmentOptions.Where(segment => segment.Contains(new TrackPoint(trackPoint.Latitude, trackPoint.Longitude, trackPoint.Altitude, ZwiftWorldId.Watopia))).ToList();

                    if (matchingSegments.Count != 1)
                    {
                        continue;
                    }

                    if (nextSegmentOptions.Length > 1 && matchingSegments.Count == 1)
                    {
                        if (Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }
                    }

                    currentSegment = matchingSegments.First();
                    currentSegment.Contains(trackPoint, out var matchingTrackPoint);
                    firstMatchOnSegment = matchingTrackPoint;
                    lastMatchOnSegment = matchingTrackPoint;
                    nextSegmentOptions = null;
                }
            }

            if (currentSegment != null && 
                result.RouteSegmentSequence[^1].SegmentId != currentSegment.Id)
            {
                result.RouteSegmentSequence[^1].NextSegmentId = currentSegment.Id;

                result.RouteSegmentSequence[^1].TurnToNextSegment =
                    result.RouteSegmentSequence[^1].Direction == SegmentDirection.AtoB
                        ? previousSegment.NextSegmentsNodeB.Single(n => n.SegmentId == currentSegment.Id)
                            .Direction
                        : previousSegment.NextSegmentsNodeA.Single(n => n.SegmentId == currentSegment.Id)
                            .Direction;
            }

            var startingSegmentId = result.RouteSegmentSequence[0].SegmentId;
            var spawnPointSegment = world.SpawnPoints.SingleOrDefault(spawn => spawn.SegmentId == startingSegmentId && spawn.Direction == result.RouteSegmentSequence[0].Direction);
            if (spawnPointSegment == null)
            {
                throw new Exception($"Route doesn't start at a known spawn point in {world.Name}");
            }

            result.ZwiftRouteName = spawnPointSegment.ZwiftRouteName;

            if (result.RouteSegmentSequence[0].SegmentId == result.RouteSegmentSequence[^1].NextSegmentId)
            {
                foreach (var seg in result.RouteSegmentSequence)
                {
                    seg.Type = SegmentSequenceType.Loop;
                }

                result.RouteSegmentSequence[0].Type = SegmentSequenceType.LoopStart;
                result.RouteSegmentSequence[^1].Type = SegmentSequenceType.LoopEnd;
            }

            routeStore.Store(result, @"c:\temp\result.json");
            result.Should().BeEquivalentTo(expectedPlannedRoute);
        }
    }
}


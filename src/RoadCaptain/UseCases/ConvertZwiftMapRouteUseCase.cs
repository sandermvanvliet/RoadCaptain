// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Linq;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class ConvertZwiftMapRouteUseCase
    {
        private readonly IWorldStore _worldStore;
        private readonly ISegmentStore _segmentStore;

        public ConvertZwiftMapRouteUseCase(IWorldStore worldStore, ISegmentStore segmentStore)
        {
            _worldStore = worldStore;
            _segmentStore = segmentStore;
        }

        public PlannedRoute Execute(ZwiftMapRoute zwiftMapRoute)
        {
            if (string.IsNullOrEmpty(zwiftMapRoute.WorldName))
            {
                throw new ArgumentException("ZwiftMap route doesn't have a world");
            }

            var world = _worldStore.LoadWorldById(zwiftMapRoute.WorldName);

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

            var segments = _segmentStore.LoadSegments(world, SportType.Cycling);

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
                        var direction = currentSegment.DirectionOf(firstMatchOnSegment!, lastMatchOnSegment!);

                        result.RouteSegmentSequence.Add(new SegmentSequence(currentSegment.Id, direction));

                        if (result.RouteSegmentSequence.Count > 1)
                        {
                            result.RouteSegmentSequence[^2].NextSegmentId = currentSegment.Id;
                            result.RouteSegmentSequence[^2].TurnToNextSegment =
                                result.RouteSegmentSequence[^2].Direction == SegmentDirection.AtoB
                                    ? previousSegment!.NextSegmentsNodeB.Single(n => n.SegmentId == currentSegment.Id)
                                        .Direction
                                    : previousSegment!.NextSegmentsNodeA.Single(n => n.SegmentId == currentSegment.Id)
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
                    var matchingSegments = nextSegmentOptions.Where(segment =>
                        segment.Contains(new TrackPoint(trackPoint.Latitude, trackPoint.Longitude, trackPoint.Altitude,
                            ZwiftWorldId.Watopia))).ToList();

                    if (matchingSegments.Count != 1)
                    {
                        continue;
                    }

                    currentSegment = matchingSegments.First();
                    currentSegment.Contains(trackPoint, out var matchingTrackPoint);
                    firstMatchOnSegment = matchingTrackPoint;
                    lastMatchOnSegment = matchingTrackPoint;
                    nextSegmentOptions = null;
                }
            }

            if (currentSegment != null &&
                previousSegment != null &&
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
            var spawnPointSegment = world.SpawnPoints!.SingleOrDefault(spawn =>
                spawn.SegmentId == startingSegmentId && spawn.Direction == result.RouteSegmentSequence[0].Direction);
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

            return result;
        }
    }
}


// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;
using Avalonia;
using Codenizer.Avalonia.Map;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared.Controls;
using SkiaSharp;
using Point = Avalonia.Point;

namespace RoadCaptain.App.RouteBuilder.Views
{
    public class MapObjectsSource
    {
        private readonly Map _map;

        public MapObjectsSource(Map map)
        {
            _map = map;
        }

        public void ToggleClimbs(bool visible)
        {
            _map.MapObjects.OfType<ClimbSegment>()
                .ToList()
                .ForEach(climb => climb.IsVisible = visible);

            InvalidateZwiftMap();
        }

        public void ToggleSprints(bool visible)
        {
            _map.MapObjects.OfType<SprintSegment>()
                .ToList()
                .ForEach(climb => climb.IsVisible = visible);

            InvalidateZwiftMap();
        }

        private void InvalidateZwiftMap()
        {
            _map.InvalidateVisual();
        }

        public void SynchronizeRouteSegmentsOnZwiftMap(RouteViewModel viewModelRoute)
        {
            using var updateScope = _map.BeginUpdate();

            var mapSegments = _map
                .MapObjects
                .OfType<MapSegment>()
                .ToList();

            var routeHasSegments = viewModelRoute.Sequence.Any();

            foreach (var spawnPoint in _map.MapObjects.OfType<SpawnPointSegment>())
            {
                spawnPoint.IsVisible = !routeHasSegments;
            }

            foreach (var segment in mapSegments)
            {
                var seq = viewModelRoute.Sequence.FirstOrDefault(s => s.SegmentId == segment.SegmentId);

                // Segments can only be selected when a spawn point has been selected as
                // the first segment on a route. To ensure that the map control doesn't attempt
                // to match on segments we can't select at all anyway we disable selection
                // here.
                segment.IsSelectable = routeHasSegments;

                if (seq != null)
                {
                    segment.IsLeadIn = seq.Type == SegmentSequenceType.LeadIn;
                    segment.IsLeadOut = seq.Type == SegmentSequenceType.LeadOut;
                    segment.IsLoop = seq.Type is SegmentSequenceType.Loop or SegmentSequenceType.LoopEnd or SegmentSequenceType.LoopStart;
                    segment.IsOnRoute = true;
                }
                else if (segment.IsOnRoute)
                {
                    segment.IsOnRoute = false;
                }
            }

            // Synchronize the route path with the route
            SynchronizeRoutePath(mapSegments, viewModelRoute);
        }

        private void SynchronizeRoutePath(List<MapSegment> mapSegments, RouteViewModel viewModelRoute)
        {
            var routePath = _map.MapObjects.SingleOrDefault(mo => mo is RoutePath);

            if (routePath != null)
            {
                _map.MapObjects.Remove(routePath);
            }

            var routePathPoints = RoutePathPointsFrom(viewModelRoute.Sequence, mapSegments);

            routePath = new RoutePath(routePathPoints);
            _map.MapObjects.Add(routePath);
        }

        private static SKPoint[] RoutePathPointsFrom(IEnumerable<SegmentSequenceViewModel> routeSequence,
            List<MapSegment> mapSegments)
        {
            if (!mapSegments.Any())
            {
                return Array.Empty<SKPoint>();
            }

            var routePoints = new List<SKPoint>();

            foreach (var seq in routeSequence)
            {
                var points = mapSegments.Single(s => s.SegmentId == seq.SegmentId).Points;

                if (seq.Direction == SegmentDirection.BtoA)
                {
                    // Don't call Reverse() because that does an
                    // in-place reverse and given that we're 
                    // _referencing_ the list of points of the
                    // segment that means that the actual segment
                    // is modified. Reverse() does not return a
                    // new IEnumerable<T>
                    points = points.AsEnumerable().Reverse().ToArray();
                }

                routePoints.AddRange(points);
            }

            return routePoints.ToArray();
        }

        public void HighlightOnZwiftMap(Segment? highlightedSegment)
        {
            var highlightedSegments = _map
                .MapObjects
                .OfType<MapSegment>()
                .ToList();

            var highlightedSegmentId = (highlightedSegment?.Id ?? "no selection");

            foreach (var segment in highlightedSegments)
            {
                if (segment.SegmentId == highlightedSegmentId)
                {
                    segment.IsHighlighted = true;
                }
                else
                {
                    segment.IsHighlighted = false;
                }
            }

            // Force re-render
            InvalidateZwiftMap();
        }

        private void AddPathsToMap(World world, List<Segment> segments, List<Segment> markers)
        {
            if (!segments.Any())
            {
                return;
            }

            var map = _map.MapObjects.SingleOrDefault(mo => mo is WorldMap);

            if (map == null)
            {
                return;
            }

            var offsets = CreatePathsForSegments(segments, world);

            if (offsets == null)
            {
                return;
            }

            CreatePathsForMarkers(
                offsets,
                markers.Where(m => m.Type == SegmentType.Climb).ToList(),
                (id, points) => new ClimbSegment(id, points));

            CreatePathsForMarkers(
                offsets,
                markers.Where(m => m.Type == SegmentType.Sprint).ToList(),
                (id, points) => new SprintSegment(id, points));
        }

        private void CreatePathsForMarkers(Offsets offsets, List<Segment> markers, Func<string, SKPoint[], MapObject> createMapObject)
        {
            var segmentsWithOffsets = markers
                .Select(seg => new
                {
                    Segment = seg,
                    GameCoordinates = seg.Points.Select(point => point.ToMapCoordinate()).ToList()
                })
            .ToList();

            foreach (var segment in segmentsWithOffsets)
            {
                var segmentPath = SkiaPathFromSegment(offsets, segment.GameCoordinates);

                var path = createMapObject(segment.Segment.Id, segmentPath.Points);
                _map.MapObjects.Add(path);
            }
        }

        private Offsets? CreatePathsForSegments(List<Segment> segments, World world)
        {
            if (world.SpawnPoints == null)
            {
                throw new ArgumentException("Can't create paths if the spawn points for the world are missing");
            }

            if (!world.MapMostLeft.HasValue || !world.MapMostRight.HasValue)
            {
                throw new ArgumentException("Can't create paths if the bounding box for the world is missing");
            }

            var size = new Rect(
                new Point(
                    world.MapMostLeft.Value.X,
                    world.MapMostLeft.Value.Y),
                new Point(
                    world.MapMostRight.Value.X,
                    world.MapMostRight.Value.Y));

            if (!segments.Any())
            {
                return null;
            }

            var segmentsWithOffsets = segments
                .Select(seg => new
                {
                    Segment = seg,
                    GameCoordinates = seg.Points.Select(point => point.ToMapCoordinate()).ToList()
                })
                .Select(x => new
                {
                    x.Segment,
                    x.GameCoordinates,
                    Offsets = new Offsets((float)size.Width, (float)size.Height, x.GameCoordinates, world.ZwiftId)
                })
                .ToList();

            var overallOffsets = Offsets
                .From(segmentsWithOffsets.Select(s => s.Offsets).ToList())
                .Translate((int)size.Left, (int)size.Top);

            foreach (var segment in segmentsWithOffsets)
            {
                var segmentPath = SkiaPathFromSegment(overallOffsets, segment.GameCoordinates);

                var path = new MapSegment(segment.Segment.Id, segmentPath.Points);
                _map.MapObjects.Add(path);
            }

            var spawnPoints = world
                .SpawnPoints
                .GroupBy(
                    s => s.SegmentId,
                    s => s,
                    (segmentId, spawnPoints) => new
                    {
                        SegmentId = segmentId,
                        SpawnPoints = spawnPoints.ToList()
                    })
                .ToList();
            
            foreach (var spawnPoint in spawnPoints)
            {
                var segmentPath = _map.MapObjects.OfType<MapSegment>()
                    .SingleOrDefault(mo => mo.SegmentId == spawnPoint.SegmentId);

                if (segmentPath != null)
                {
                    var spawnPointSegment = segments.Single(s => s.Id == spawnPoint.SegmentId);
                    var offset = 4;
                    var middle = spawnPointSegment.Points.Count / 2;

                    var startPoint = spawnPointSegment.Points[middle - offset];
                    var endPoint = spawnPointSegment.Points[middle + offset];

                    var middleBearing = TrackPoint.Bearing(startPoint, endPoint);
                    var isTwoWay = false;

                    if(spawnPoint.SpawnPoints.Count == 1 && spawnPoint.SpawnPoints[0].Direction == SegmentDirection.BtoA)
                    {
                        // If the spawn point goes in the opposite direction
                        // of the segment then flip the bearing
                        middleBearing = (middleBearing + 180) % 360;
                    }
                    else if (spawnPoint.SpawnPoints.Select(s => s.Direction).Distinct().Count() > 1)
                    {
                        isTwoWay = true;
                    }

                    _map.MapObjects.Add(new SpawnPointSegment(segmentPath.SegmentId, segmentPath.Points, middleBearing, isTwoWay));
                }
            }

            return overallOffsets;
        }

        private static SKPath SkiaPathFromSegment(Offsets offsets, List<MapCoordinate> data)
        {
            var path = new SKPath();

            path.AddPoly(
                data
                    .Select(offsets.ScaleAndTranslate)
                    .Select(point => new SKPoint(point.X, point.Y))
                    .ToArray(),
                false);

            return path;
        }

        public void SetZwiftMap(RouteViewModel viewModelRoute, List<Segment> segments, List<Segment> markers)
        {
            var stopwatch = Stopwatch.StartNew();

            var currentMap =
                _map.MapObjects.SingleOrDefault(mo => mo is WorldMap);

            var worldId = viewModelRoute.World?.Id;

            if (string.IsNullOrEmpty(worldId) && currentMap != null)
            {
                _map.MapObjects.Clear();
            }

            if (!string.IsNullOrEmpty(worldId))
            {
                if (currentMap != null && !currentMap.Name.EndsWith($"-{worldId}"))
                {
                    Debug.WriteLine("Clearing objects from map because a different world was selected");
                    _map.MapObjects.Clear();
                }

                if (currentMap == null)
                {
                    _map.MapObjects.Add(new WorldMap(worldId));
                    AddPathsToMap(viewModelRoute.World!, segments, markers);
                }
            }

            stopwatch.Stop();
            Debug.WriteLine($"SetZwiftMap(): {stopwatch.ElapsedMilliseconds}ms");
        }
    }

    public enum SegmentSelectionMode
    {
        All,
        OnlySpawnPoints
    }
}

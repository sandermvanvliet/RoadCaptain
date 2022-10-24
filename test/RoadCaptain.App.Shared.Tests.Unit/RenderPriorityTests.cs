using System;
using System.Linq;
using Codenizer.Avalonia.Map;
using FluentAssertions;
using RoadCaptain.App.Shared.Controls;
using SkiaSharp;
using Xunit;

namespace RoadCaptain.App.Shared.Tests.Unit
{
    public class RenderPriorityTests
    {
        [Fact]
        public void GivenWorldMapAndRoutePath_SequenceIsWorldMapRoutePath()
        {
            var input = new[] { GivenWorldMap(), GivenRoutePath() };

            var output = input.OrderBy(mo => mo, new ZwiftMapRenderPriority()).ToList();

            output
                .Select(x => x.GetType())
                .ToList()
                .Should()
                .ContainInOrder(typeof(WorldMap), typeof(RoutePath));
        }

        [Fact]
        public void GivenRoutePathAndWorldMap_SequenceIsWorldMapRoutePath()
        {
            var input = new[] { GivenRoutePath(), GivenWorldMap() };

            var output = input.OrderBy(mo => mo, new ZwiftMapRenderPriority()).ToList();

            output
                .Select(x => x.GetType())
                .ToList()
                .Should()
                .ContainInOrder(typeof(WorldMap), typeof(RoutePath));
        }

        [Fact]
        public void GivenRoutePathAnMapSegment_SequenceIsMapSegmentRoutePath()
        {
            var input = new[] { GivenRoutePath(), GivenMapSegment() };

            var output = input.OrderBy(mo => mo, new ZwiftMapRenderPriority()).ToList();

            Enumerable.ToList<Type>(output
                    .Select(x => x.GetType()))
                .Should()
                .ContainInOrder(typeof(MapSegment), typeof(RoutePath));
        }

        [Fact]
        public void GivenSpawnPointAndMapSegment_SequenceIsMapSegmentSpawnPoint()
        {
            var input = new MapObject[] { GivenSpawnPointSegment(), GivenMapSegment() };

            var output = input.OrderBy(mo => mo, new ZwiftMapRenderPriority()).ToList();

            output
                .Select(x => x.GetType())
                .ToList<Type>()
                .Should()
                .ContainInOrder(typeof(MapSegment), typeof(SpawnPointSegment));
        }

        private static MapSegment GivenMapSegment()
        {
            return new MapSegment("watopia", Array.Empty<SKPoint>());
        }

        private static SpawnPointSegment GivenSpawnPointSegment()
        {
            return new SpawnPointSegment("x", Array.Empty<SKPoint>());
        }

        private MapObject GivenRoutePath()
        {
            return new RoutePath(Array.Empty<SKPoint>());
        }

        private static WorldMap GivenWorldMap()
        {
            return new WorldMap("watopia");
        }
    }
}
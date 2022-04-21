using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.RouteBuilder.ViewModels;
using RoadCaptain.UserInterface.Shared.Commands;
using SkiaSharp;
using Xunit;

namespace RoadCaptain.RouteBuilder.Tests.Unit
{
    public class WhenPlanningRoute
    {
        private readonly TestableMainWindowViewModel _viewModel;
        private readonly SegmentStore _segmentStore;
        private List<Segment> _segments;
        private readonly WorldStoreToDisk _worldStore;

        public WhenPlanningRoute()
        {
            _segmentStore = new SegmentStore();
            var worldStore = new WorldStoreToDisk();

            _worldStore = worldStore;
            _viewModel = new TestableMainWindowViewModel(
                new RouteStoreToDisk(_segmentStore, _worldStore),
                _segmentStore,
                null,
                new StubWindowService(),
                _worldStore,
                new UserPreferences());
        }

        private void GivenWorldAndSport(string worldId, SportType sportType)
        {
            _viewModel.SelectWorldCommand.Execute(new WorldViewModel(_worldStore.LoadWorldById(worldId)));
            _viewModel.SelectSportCommand.Execute(new SportViewModel(sportType));
            _viewModel.CreatePathsForSegments(800, 600);
            _segments = _segmentStore.LoadSegments(_viewModel.Route.World, _viewModel.Route.Sport);
        }

        [Theory]
        [InlineData("watopia-big-foot-hills-004-before", "watopia-big-foot-hills-007-after", SegmentDirection.BtoA, SegmentDirection.BtoA)]
        [InlineData("watopia-big-foot-hills-004-before", "watopia-big-foot-hills-004-after-before", SegmentDirection.AtoB, SegmentDirection.AtoB)]
        [InlineData("watopia-bambino-fondo-001-after-after-after-after-after-after", "watopia-bambino-fondo-001-after-before", SegmentDirection.AtoB, SegmentDirection.AtoB)]
        [InlineData("watopia-bambino-fondo-001-after-after-after-after-after-after", "watopia-bambino-fondo-001-after-after-after-after-after-before", SegmentDirection.BtoA, SegmentDirection.BtoA)]
        public void GivenStartingPointAndNextSegment_RouteDirectionisCorrect(string startingSegmentId, string nextSegmentId, SegmentDirection expectedFirstSequenceDirection, SegmentDirection expectedSecondSequenceDirection)
        {
            GivenWorldAndSport("watopia", SportType.Cycling);

            _viewModel.CallAddSegmentToRoute(GetSegmentById(startingSegmentId));
            _viewModel.CallAddSegmentToRoute(GetSegmentById(nextSegmentId));

            _viewModel.Route.Sequence.First().Direction.Should().Be(expectedFirstSequenceDirection);
            _viewModel.Route.Sequence.Skip(1).First().Direction.Should().Be(expectedSecondSequenceDirection);
        }

        [Fact]
        public void GivenSpawnPointWithDirectionAtoBAndInvalidNextSegment_ErrorIsReturned()
        {
            GivenWorldAndSport("watopia", SportType.Cycling);

            _viewModel.CallAddSegmentToRoute(GetSegmentById("watopia-beach-island-loop-001")); // This segment only has AtoB as a valid spawn direction
            var result = _viewModel.CallAddSegmentToRoute(GetSegmentById("watopia-bambino-fondo-001-after-after-after-after-before-after"));

            result
                .Result
                .Should()
                .Be(Result.Failure);

            result
                .Message
                .Should()
                .Be("Spawn point does not support the direction of the selected segment");
        }

        [Fact]
        public void GivenSpawnPointWithDirectionBtoAAndInvalidNextSegment_ErrorIsReturned()
        {
            GivenWorldAndSport("watopia", SportType.Running);

            _viewModel.CallAddSegmentToRoute(GetSegmentById("watopia-5k-loop-001-after-after-before-after")); // This segment only has AtoB as a valid spawn direction
            var result = _viewModel.CallAddSegmentToRoute(GetSegmentById("watopia-5k-loop-001-after-after-after"));

            result
                .Result
                .Should()
                .Be(Result.Failure);

            result
                .Message
                .Should()
                .Be("Spawn point does not support the direction of the selected segment");
        }

        [Fact]
        public void GivenSpawnPointAndValidNextSegment_SuccessIsReturned()
        {
            GivenWorldAndSport("watopia", SportType.Cycling);

            _viewModel.CallAddSegmentToRoute(GetSegmentById("watopia-beach-island-loop-001")); // This segment only has AtoB as a valid spawn direction
            var result = _viewModel.CallAddSegmentToRoute(GetSegmentById("watopia-bambino-fondo-004-before-after"));

            result
                .Result
                .Should()
                .Be(Result.SuccessWithWarnings);

            result
                .Message
                .Should()
                .Be("Volcano circuit 3");
        }

        [Fact]
        public void GivenSpawnPointAndValidNextSegmentAlternate_SuccessIsReturned()
        {
            GivenWorldAndSport("watopia", SportType.Cycling);

            _viewModel.CallAddSegmentToRoute(GetSegmentById("watopia-beach-island-loop-001")); // This segment only has AtoB as a valid spawn direction
            var result = _viewModel.CallAddSegmentToRoute(GetSegmentById("watopia-bambino-fondo-004-after-before"));

            result
                .Result
                .Should()
                .Be(Result.SuccessWithWarnings);

            result
                .Message
                .Should()
                .Be("Volcano circuit 1");
        }

        // 1: watopia-bambino-fondo-001-after-after-after-after-after-before
        // 2: watopia-bambino-fondo-001-after-after-after-after-before-after
        [Fact]
        public void GivenAtoBSegmentAndRouteStartsInBtoADirection_RoutePathIsGeneratedCorrectly()
        {
            GivenWorldAndSport("watopia", SportType.Cycling);

            _viewModel.CallAddSegmentToRoute(GetSegmentById("watopia-bambino-fondo-001-after-after-after-after-after-before"));
            _viewModel.CallAddSegmentToRoute(GetSegmentById("watopia-bambino-fondo-001-after-after-after-after-before-after"));

            // This is a bit convoluted but given that we know that:
            // 1: Segment watopia-bambino-fondo-001-after-after-after-after-after-before is AtoB
            // 2: The route segments have directions BtoA -> BtoA
            // 3: Therefore the first part of the route is the 'B' part of the first segment
            var expectedFirstPoint = _viewModel.SegmentPaths[_viewModel.Route.Sequence.First().SegmentId].Points.Last();

            _viewModel.RoutePath.Points.First().Should().Be(expectedFirstPoint);
        }

        private Segment GetSegmentById(string id)
        {
            return _segments.Single(s => s.Id == id);
        }
    }
}
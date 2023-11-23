// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.App.RouteBuilder.Services;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared.Commands;
using Xunit;

namespace RoadCaptain.App.RouteBuilder.Tests.Unit
{
    public class WhenPlanningRoute
    {
        private readonly TestableMainWindowViewModel _viewModel;
        private readonly SegmentStore _segmentStore;
        private List<Segment>? _segments;
        private readonly WorldStoreToDisk _worldStore;

        public WhenPlanningRoute()
        {
            _segmentStore = new SegmentStore();
            var worldStore = new WorldStoreToDisk();

            _worldStore = worldStore;
            _viewModel = new TestableMainWindowViewModel(
                new RouteStoreToDisk(_segmentStore, _worldStore),
                _segmentStore,
                new DummyVersionChecker(),
                new StubWindowService(new ContainerBuilder().Build(), new NopMonitoringEvents()),
                _worldStore,
                new TestUserPreferences(),
                new DummyApplicationFeatures(), 
                new StatusBarService(),
                null!, null!);
        }

        private void GivenWorldAndSport(string worldId, SportType sportType)
        {
            var world = _worldStore.LoadWorldById(worldId);
            world.Should().NotBeNull();
            // _viewModel.SelectWorldCommand.Execute(new WorldViewModel(world!));
            // _viewModel.SelectSportCommand.Execute(new SportViewModel(sportType));
            _segments = _segmentStore.LoadSegments(_viewModel.Route.World!, _viewModel.Route.Sport);
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
                .Should()
                .BeOfType<CommandResultWithMessage>()
                .Which
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
                .Should()
                .BeOfType<CommandResultWithMessage>()
                .Which
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
                .Be(Result.SuccessWithMessage);

            result
                .Should()
                .BeOfType<CommandResultWithMessage>()
                .Which
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
                .Be(Result.SuccessWithMessage);

            result
                .Should()
                .BeOfType<CommandResultWithMessage>()
                .Which
                .Message
                .Should()
                .Be("Volcano circuit 1");
        }

        [Fact]
        public void GivenSegmentWhichIsSpawnPointForOnlyOneRoute_DirectionIsSetOnFirstSequenceItem()
        {
            GivenWorldAndSport("watopia", SportType.Cycling);

            _viewModel.CallAddSegmentToRoute(GetSegmentById("watopia-bambino-fondo-004-before-before"));

            _viewModel
                .Route
                .Sequence
                .Single()
                .Direction
                .Should()
                .Be(SegmentDirection.BtoA);
        }

        [Fact]
        public void GivenSegmentWhichIsSpawnPointForMultipleRoutesAndDirections_DirectionIsNotSet()
        {
            GivenWorldAndSport("watopia", SportType.Cycling);
            
            _viewModel.CallAddSegmentToRoute(GetSegmentById("watopia-bambino-fondo-001-after-after-after-after-after-before"));

            _viewModel
                .Route
                .Sequence
                .Single()
                .Direction
                .Should()
                .Be(SegmentDirection.Unknown);
        }

        [Fact]
        public void GivenSegmentWhichIsSpawnPointForMultipleRoutesWithSameDirections_DirectionIsSetOnFirstSequenceItem()
        {
            GivenWorldAndSport("watopia", SportType.Cycling);
            
            _viewModel.CallAddSegmentToRoute(GetSegmentById("watopia-beach-island-loop-001"));

            _viewModel
                .Route
                .Sequence
                .Single()
                .Direction
                .Should()
                .Be(SegmentDirection.AtoB);
        }

        private Segment GetSegmentById(string id)
        {
            return _segments!.Single(s => s.Id == id);
        }
    }
}

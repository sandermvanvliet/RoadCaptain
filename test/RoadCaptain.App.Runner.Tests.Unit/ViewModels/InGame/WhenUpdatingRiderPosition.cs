using System.Collections.Generic;
using FluentAssertions;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.ViewModels.InGame
{
    public class WhenUpdatingRiderPosition
    {
        private readonly InGameNavigationWindowViewModel _viewModel;
        private readonly Segment _segmentOne;
        private readonly TrackPoint _positionOne = new(1,2,3);
        private readonly TrackPoint _positionTwo = new(1,2.0001,4); // 1m ascent
        private readonly TrackPoint _positionThree = new(1,2.0002,2); // 2m descent
        private readonly PlannedRoute _route;

        [Fact]
        public void GivenOnSegmentStateWithElapsedDistanceAscentAndDescent_ElapsedDistanceIsSet()
        {
            var onSegmentState = new OnSegmentState(1, 2, _positionOne, _segmentOne, SegmentDirection.AtoB, 0 ,0, 0);
            var result = onSegmentState.UpdatePosition(_positionTwo,
                new List<Segment> { _segmentOne }, _route);

            WhenUpdating(result);

            _viewModel.Model.ElapsedDistance.Should().NotBe(0);
        }

        [Fact]
        public void GivenOnSegmentStateWithElapsedDistanceAscentAndDescent_ElapsedAscentIsSet()
        {
            var onSegmentState = new OnSegmentState(1, 2, _positionOne, _segmentOne, SegmentDirection.AtoB, 0 ,0, 0);
            var result = onSegmentState.UpdatePosition(_positionTwo,
                new List<Segment> { _segmentOne }, _route);

            WhenUpdating(result);

            _viewModel.Model.ElapsedAscent.Should().NotBe(0);
        }

        [Fact]
        public void GivenOnSegmentStateWithElapsedDistanceAscentAndDescent_ElapsedDescentIsSet()
        {
            var onSegmentState = new OnSegmentState(1, 2, _positionTwo, _segmentOne, SegmentDirection.AtoB, 0 ,0, 0);
            var result = onSegmentState.UpdatePosition(_positionThree,
                new List<Segment> { _segmentOne }, _route);

            WhenUpdating(result);

            _viewModel.Model.ElapsedDescent.Should().NotBe(0);
        }
            
        public WhenUpdatingRiderPosition()
        {
            _segmentOne = new(new List<TrackPoint>
            {
                _positionOne,
                _positionTwo,
                _positionThree
            })
            {
                Id = "seg-1"
            };

            _segmentOne.CalculateDistances();
            
            var segments = new List<Segment>
            {
                _segmentOne
            };

            _route = new PlannedRoute
            {
                World = new World { Id = "testworld", Name = "TestWorld" },
                RouteSegmentSequence =
                {
                    new SegmentSequence
                    {
                        Direction = SegmentDirection.AtoB,
                        SegmentId = _segmentOne.Id,
                        TurnToNextSegment = TurnDirection.None,
                        NextSegmentId = null
                    }
                }
            };

            _route.EnteredSegment(_segmentOne.Id);

            var inGameWindowModel = new InGameWindowModel(segments)
            {
                Route = _route
            };

            _viewModel = new InGameNavigationWindowViewModel(inGameWindowModel, segments, null);
        }

        private void WhenUpdating(GameState gameState)
        {
            _viewModel.UpdateGameState(gameState);
        }
    }
}
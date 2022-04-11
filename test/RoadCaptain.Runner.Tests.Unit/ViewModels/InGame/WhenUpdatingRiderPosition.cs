using System.Collections.Generic;
using FluentAssertions;
using RoadCaptain.GameStates;
using RoadCaptain.Runner.Models;
using RoadCaptain.Runner.ViewModels;
using Xunit;

namespace RoadCaptain.Runner.Tests.Unit.ViewModels.InGame
{
    public class WhenUpdatingRiderPosition
    {
        private const double DistanceBetweenOneAndTwo = 0.011117799111888066;
        private const double DistanceBetweenFourAndFive = 0.011117799111838694;
        private readonly InGameNavigationWindowViewModel _viewModel;
        private readonly Segment _segmentOne;
        private readonly TrackPoint _positionOne = new(1,2,3);
        private readonly TrackPoint _positionTwo = new(1,2.0001,4); // 1m ascent
        private readonly TrackPoint _positionThree = new(1,2.0002,2); // 2m descent
        private readonly TrackPoint _positionFour = new(1,2.0003,6); // 4m descent
        private readonly TrackPoint _positionFive = new(1,2.0004,3); // 3m descent
        private readonly TrackPoint _positionSix = new(1,2.0005,4); // 1m ascent
        private readonly TrackPoint _positionSeven = new(1,2.0006,2); // 2m descent
        private readonly TrackPoint _positionEight = new(1,2.0007,6); // 4m descent
        private readonly Segment _segmentTwo;
        private readonly PlannedRoute _route;
        private readonly Segment _segmentThree;

        [Fact]
        public void GivenNoPreviousPosition_ElapsedDistanceIsNotChanged()
        {
            _viewModel.Model.ElapsedDistance = 123;

            WhenUpdatingToPositionOnSegment(_positionOne);

            _viewModel.Model.ElapsedDistance.Should().Be(123);
        }

        [Fact]
        public void GivenNoPreviousPosition_ElapsedAscentIsNotChanged()
        {
            _viewModel.Model.ElapsedAscent = 123;
            
            WhenUpdatingToPositionOnSegment(_positionOne);

            _viewModel.Model.ElapsedAscent.Should().Be(123);
        }

        [Fact]
        public void GivenNoPreviousPosition_ElapsedDescentIsNotChanged()
        {
            _viewModel.Model.ElapsedDescent = 123;

            WhenUpdatingToPositionOnSegment(_positionOne);

            _viewModel.Model.ElapsedDescent.Should().Be(123);
        }

        [Fact]
        public void GivenPositionChangesFromOneToTwo_ElapsedDistanceIsUpdated()
        {
            WhenUpdatingToPositionOnSegment(_positionOne);
            WhenUpdatingToPositionOnSegment(_positionTwo);

            _viewModel.Model.ElapsedDistance.Should().Be(DistanceBetweenOneAndTwo);
        }

        [Fact]
        public void GivenPositionChangesFromOneToTwo_ElapsedAscentIsUpdated()
        {
            WhenUpdatingToPositionOnSegment(_positionOne);
            WhenUpdatingToPositionOnSegment(_positionTwo);

            _viewModel.Model.ElapsedAscent.Should().Be(1);
        }

        [Fact]
        public void GivenPositionChangesFromOneToTwo_ElapsedDescentRemainsTheSame()
        {
            WhenUpdatingToPositionOnSegment(_positionOne);
            WhenUpdatingToPositionOnSegment(_positionTwo);

            _viewModel.Model.ElapsedDescent.Should().Be(0);
        }

        [Fact]
        public void GivenPositionChangesFromTwoToOne_ElapsedAscentRemainsTheSame()
        {
            WhenUpdatingToPositionOnSegment(_positionTwo);
            WhenUpdatingToPositionOnSegment(_positionOne);

            _viewModel.Model.ElapsedAscent.Should().Be(0);
        }

        [Fact]
        public void GivenPositionChangesFromTwoToOne_ElapsedDescentIsUpdated()
        {
            WhenUpdatingToPositionOnSegment(_positionTwo);
            WhenUpdatingToPositionOnSegment(_positionOne);

            _viewModel.Model.ElapsedDescent.Should().Be(1);
        }

        [Fact]
        public void GivenPositionChangesFromFourToFive_ElapsedDistanceIsUpdated()
        {
            WhenUpdatingToPositionOnSegment(_positionFour);
            WhenUpdatingToPositionOnNextSegment(_positionFive);

            _viewModel.Model.ElapsedDistance.Should().Be(DistanceBetweenFourAndFive);
        }

        [Fact]
        public void GivenPositionChangesFromFourToFive_CurrentSegmentIsChanged()
        {
            WhenUpdatingToPositionOnSegment(_positionFour);

            _route.EnteredSegment(_segmentTwo.Id);
            WhenUpdatingToPositionOnNextSegment(_positionFive);

            _viewModel.Model.CurrentSegment.SegmentId.Should().Be(_segmentTwo.Id);
        }

        [Fact]
        public void GivenPositionChangesFromFourToFive_NextSegmentIsSegmentThree()
        {
            WhenUpdatingToPositionOnSegment(_positionFour);

            _route.EnteredSegment(_segmentTwo.Id);
            WhenUpdatingToPositionOnNextSegment(_positionFive);

            _viewModel.Model.NextSegment.SegmentId.Should().Be(_segmentThree.Id);
        }

        [Fact]
        public void GivenPositionChangesFromSixToSeven_NextSegmentIsSegmentThree()
        {
            WhenUpdatingToPositionOnSegment(_positionFour);
            
            _route.EnteredSegment(_segmentTwo.Id);
            WhenUpdatingToPositionOnNextSegment(_positionSix);

            _route.EnteredSegment(_segmentThree.Id);
            WhenUpdatingToPositionOnNextSegment(_positionSeven, _segmentThree);

            _viewModel.Model.NextSegment.Should().BeNull();
        }
            
        public WhenUpdatingRiderPosition()
        {
            _segmentOne = new(new List<TrackPoint>
            {
                _positionOne,
                _positionTwo,
                _positionThree,
                _positionFour
            })
            {
                Id = "seg-1"
            };

            _segmentOne.CalculateDistances();

            _segmentTwo = new(new List<TrackPoint>
            {
                _positionFive,
                _positionSix
            })
            {
                Id = "seg-2"
            };

            _segmentTwo.CalculateDistances();

            _segmentThree = new(new List<TrackPoint>
            {
                _positionSeven,
                _positionEight
            })
            {
                Id = "seg-3"
            };

            _segmentThree.CalculateDistances();

            _segmentOne.NextSegmentsNodeB.Add(new Turn(TurnDirection.GoStraight, _segmentTwo.Id));
            _segmentTwo.NextSegmentsNodeA.Add(new Turn(TurnDirection.GoStraight, _segmentOne.Id));
            _segmentTwo.NextSegmentsNodeB.Add(new Turn(TurnDirection.GoStraight, _segmentThree.Id));
            _segmentThree.NextSegmentsNodeA.Add(new Turn(TurnDirection.GoStraight, _segmentTwo.Id));

            var segments = new List<Segment>
            {
                _segmentOne,
                _segmentTwo,
                _segmentThree
            };

            _route = new PlannedRoute
            {
                World = "TestWorld",
                RouteSegmentSequence =
                {
                    new SegmentSequence
                    {
                        Direction = SegmentDirection.AtoB,
                        SegmentId = _segmentOne.Id,
                        TurnToNextSegment = TurnDirection.GoStraight,
                        NextSegmentId = _segmentTwo.Id
                    },
                    new SegmentSequence
                    {
                        Direction = SegmentDirection.AtoB,
                        SegmentId = _segmentTwo.Id,
                        NextSegmentId = _segmentThree.Id
                    },
                    new SegmentSequence
                    {
                        Direction = SegmentDirection.AtoB,
                        SegmentId = _segmentThree.Id
                    }
                }
            };

            _route.EnteredSegment(_segmentOne.Id);

            var inGameWindowModel = new InGameWindowModel(segments)
            {
                Route = _route
            };

            _viewModel = new InGameNavigationWindowViewModel(inGameWindowModel, segments);
        }

        private void WhenUpdating(GameState gameState)
        {
            _viewModel.UpdateGameState(gameState);
        }

        private void WhenUpdatingToPositionOnSegment(TrackPoint position)
        {
            WhenUpdating(new OnRouteState(1, 1, position, _segmentOne, _route));
        }

        private void WhenUpdatingToPositionOnNextSegment(TrackPoint position, Segment segment = null)
        {
            WhenUpdating(new OnRouteState(1, 1, position, segment ?? _segmentTwo, _route));
        }
    }
}
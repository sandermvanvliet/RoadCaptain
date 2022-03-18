using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.GameStates;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.Tests.Unit
{
    public class WhenNavigating
    {
        private readonly NavigationUseCase _useCase;
        private readonly InMemoryZwiftGameConnection _inMemoryZwiftGameConnection;
        private readonly PlannedRoute _plannedRoute;
        private readonly InMemoryGameStateDispatcher _gameStateDispatcher;

        public WhenNavigating()
        {
            var monitoringEvents = new NopMonitoringEvents();
            _gameStateDispatcher = new InMemoryGameStateDispatcher(monitoringEvents);
            _plannedRoute = FixedForTesting();

            _inMemoryZwiftGameConnection = new InMemoryZwiftGameConnection();
            _useCase = new NavigationUseCase(
                _gameStateDispatcher,
                monitoringEvents,
                _inMemoryZwiftGameConnection);
        }
        
        public static PlannedRoute FixedForTesting()
        {
            var route = new SegmentSequenceBuilder()
                .StartingAt("seg-1")
                .TurningLeftTo("seg-2")
                .GoingStraightTo("seg-3")
                .TurningRightTo("seg-4")
                .GoingStraightTo("seg-5")
                .GoingStraightTo("seg-6")
                .TurningRightTo("seg-7")
                .EndingAt("seg-7")
                .Build();

            return route;
        }

        [Fact]
        public void GivenStartedRouteOnSegmentThreeAndLeftAndGoStraightCommandsAvailable_NoCommandIsSent()
        {
            _plannedRoute.EnteredSegment("seg-1");
            _plannedRoute.EnteredSegment("seg-2");
            _plannedRoute.EnteredSegment("seg-3");

            var state = UpcomingTurnStateWithTurns(TurnDirection.Left, TurnDirection.GoStraight);
            
            _gameStateDispatcher.Dispatch(state);

            WhenHandlingNavigation();

            _inMemoryZwiftGameConnection
                .SentCommands
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GivenStartedRouteOnSegmentThreeAndLeftAndRightCommandsAvailable_TurnRightCommandIsSent()
        {
                _plannedRoute.EnteredSegment("seg-1");
                _plannedRoute.EnteredSegment("seg-2");
                _plannedRoute.EnteredSegment("seg-3");

                var state = UpcomingTurnStateWithTurns(TurnDirection.Left, TurnDirection.Right);
            
                _gameStateDispatcher.Dispatch(state);

            WhenHandlingNavigation();

            _inMemoryZwiftGameConnection
                .SentCommands
                .Should()
                .Contain(TurnDirection.Right.ToString())
                .And
                .HaveCount(1);
        }

        [Fact]
        public void GivenStartedRouteOnLastSegment_NextSegmentIsEmpty()
        {
            var plannedRoute = FixedForTesting();

            plannedRoute.EnteredSegment("seg-1");
            plannedRoute.EnteredSegment("seg-2");
            plannedRoute.EnteredSegment("seg-3");
            plannedRoute.EnteredSegment("seg-4");
            plannedRoute.EnteredSegment("seg-5");
            plannedRoute.EnteredSegment("seg-6");
            plannedRoute.EnteredSegment("seg-7");

            plannedRoute
                .NextSegmentId
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenStartedRouteOnSegmentThreeAndEnteringSegmentFive_ArgumentExceptionIsThrown()
        {
            var plannedRoute = FixedForTesting();
            plannedRoute.EnteredSegment("seg-1");
            plannedRoute.EnteredSegment("seg-2");
            plannedRoute.EnteredSegment("seg-3");

            Action act = () => plannedRoute.EnteredSegment("seg-5");

            act.Should()
                .Throw<ArgumentException>();

            plannedRoute
                .NextSegmentId
                .Should()
                .Be("seg-4");
        }

        [Fact]
        public void GivenStartedRouteOnLastSegment_TurnToNextSegmentIsNone()
        {
            var plannedRoute = FixedForTesting();
            plannedRoute.EnteredSegment("seg-1");
            plannedRoute.EnteredSegment("seg-2");
            plannedRoute.EnteredSegment("seg-3");
            plannedRoute.EnteredSegment("seg-4");
            plannedRoute.EnteredSegment("seg-5");
            plannedRoute.EnteredSegment("seg-6");
            plannedRoute.EnteredSegment("seg-7");

            plannedRoute
                .TurnToNextSegment
                .Should()
                .Be(TurnDirection.None);
        }

        [Fact]
        public void GivenStartedRouteOnLastSegmentAndEnteringSegment_ArgumentExceptionIsThrown()
        {
            var plannedRoute = FixedForTesting();

            plannedRoute.EnteredSegment("seg-1");
            plannedRoute.EnteredSegment("seg-2");
            plannedRoute.EnteredSegment("seg-3");
            plannedRoute.EnteredSegment("seg-4");
            plannedRoute.EnteredSegment("seg-5");
            plannedRoute.EnteredSegment("seg-6");
            plannedRoute.EnteredSegment("seg-7");

            Action act = () => plannedRoute.EnteredSegment("seg-5");

            act
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("Route has already completed, can't enter new segment");
        }

        private void WhenHandlingNavigation()
        {
            var tokenSource = Debugger.IsAttached
                ? new CancellationTokenSource()
                : new CancellationTokenSource(100);

            try
            {
                _useCase.Execute(tokenSource.Token);
            }
            finally
            {
                tokenSource.Cancel();
            }
        }

        private UpcomingTurnState UpcomingTurnStateWithTurns(params TurnDirection[] directions)
        {
            return new UpcomingTurnState(
                5678,
                1234, 
                new TrackPoint(0, 0, 0), 
                new Segment(new List<TrackPoint>()) { Id = _plannedRoute.CurrentSegmentId },
                _plannedRoute, 
                SegmentDirection.AtoB, 
                directions.ToList());
        }
    }
}
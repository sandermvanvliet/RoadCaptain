// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using RoadCaptain.GameStates;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.Tests.Unit
{
    public class ZwiftKonamiCode
    {
        [Fact]
        public void LeftAndGoStraight_RouteGoStraight_LeftLeftRightGoStraightIsSent()
        {
            // Left + GoStraight from game
            // Left + Right on segment
            // Route turn: Right
            // Turn to execute: GoStraight
            // Executes: Left, Left, Right, GoStraight

            var upcomingTurnState = GivenUpcomingTurnState(TurnDirection.Right, new List<TurnDirection> { TurnDirection.Left, TurnDirection.GoStraight });

            _dispatcher.Dispatch(upcomingTurnState);

            _inMemoryZwiftGameConnection
                .SentCommands
                .Should()
                .ContainInOrder("Left", "Left", "Right", "GoStraight");
        }

        [Fact]
        public void RightAndGoStraight_RouteGoStraight_RightRightLeftGoStraightIsSent()
        {
            // Left + GoStraight from game
            // Left + Right on segment
            // Route turn: Right
            // Turn to execute: GoStraight
            // Executes: Left, Left, Right, GoStraight

            var upcomingTurnState = GivenUpcomingTurnState(TurnDirection.GoStraight, new List<TurnDirection> { TurnDirection.Right, TurnDirection.GoStraight });

            _dispatcher.Dispatch(upcomingTurnState);

            _inMemoryZwiftGameConnection
                .SentCommands
                .Should()
                .ContainInOrder("Right", "Right", "Left", "GoStraight");
        }

        [Fact]
        public void LeftAndRight_RouteToLeft_LeftIsSent()
        {
            // Left + GoStraight from game
            // Left + Right on segment
            // Route turn: Right
            // Turn to execute: GoStraight
            // Executes: Left, Left, Right, GoStraight

            var upcomingTurnState = GivenUpcomingTurnState(TurnDirection.Left, new List<TurnDirection> { TurnDirection.Right, TurnDirection.Left });

            _dispatcher.Dispatch(upcomingTurnState);

            _inMemoryZwiftGameConnection
                .SentCommands
                .Should()
                .ContainInOrder("Left");
        }

        [Fact]
        public void LeftAndRight_RouteToRight_RightIsSent()
        {
            // Left + GoStraight from game
            // Left + Right on segment
            // Route turn: Right
            // Turn to execute: GoStraight
            // Executes: Left, Left, Right, GoStraight

            var upcomingTurnState = GivenUpcomingTurnState(TurnDirection.Right, new List<TurnDirection> { TurnDirection.Right, TurnDirection.Left });

            _dispatcher.Dispatch(upcomingTurnState);

            _inMemoryZwiftGameConnection
                .SentCommands
                .Should()
                .ContainInOrder("Right");
        }

        private UpcomingTurnState GivenUpcomingTurnState(TurnDirection routeNextTurn, List<TurnDirection> gameTurns)
        {
            return new UpcomingTurnState(RiderId, ActivityId, CurrentPoint, _segment,
                GivenRouteWith(routeNextTurn),
                SegmentDirection.AtoB,
                gameTurns, 
                0, 
                0,
                0);
        }

        private PlannedRoute GivenRouteWith(TurnDirection turn)
        {
            var route = new PlannedRoute
            {
                Name = "TestRoute",
                Sport = SportType.Cycling,
                World = new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia },
                WorldId = "watopia",
                ZwiftRouteName = "TestRoute"
            };

            route.RouteSegmentSequence.Add(new SegmentSequence
            {
                SegmentId = _segment.Id,
                Direction = SegmentDirection.AtoB,
                NextSegmentId = _segment.Id + "-after",
                TurnToNextSegment = turn,
                Type = SegmentSequenceType.Regular
            });

            route.EnteredSegment(_segment.Id);
            
            return route;
        }

        private const int RiderId = 123;
        private const int ActivityId = 456;
        private readonly Segment _segment = GivenSegment();
        private static readonly TrackPoint CurrentPoint = new TrackPoint(1.23, 4.56, 100, ZwiftWorldId.Watopia);
        private readonly NavigationUseCase _useCase;
        private readonly InMemoryZwiftGameConnection _inMemoryZwiftGameConnection;
        private readonly SynchronousGameStateDispatcher _dispatcher;

        private static Segment GivenSegment()
        {
            return new Segment(new List<TrackPoint> { CurrentPoint });
        }

        public ZwiftKonamiCode()
        {
            _inMemoryZwiftGameConnection = new InMemoryZwiftGameConnection();
            _dispatcher = new SynchronousGameStateDispatcher();
            _useCase = new NavigationUseCase(
                _dispatcher,
                new NopMonitoringEvents(),
                _inMemoryZwiftGameConnection);

            _useCase.Execute(CancellationToken.None);
        }
    }
}


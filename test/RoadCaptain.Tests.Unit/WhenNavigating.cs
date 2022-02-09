using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.Tests.Unit
{
    public class WhenNavigating
    {
        private readonly InMemoryGameStateReceiver _gameStateReceiver;
        private readonly PlannedRoute _plannedRoute;
        private readonly NavigationUseCase _useCase;
        private InMemoryMessageReceiver _inMemoryMessageReceiver;

        public WhenNavigating()
        {
            var monitoringEvents = new NopMonitoringEvents();
            _gameStateReceiver = new InMemoryGameStateReceiver(monitoringEvents);

            _plannedRoute = FixedForTesting();

            _inMemoryMessageReceiver = new InMemoryMessageReceiver();
            _useCase = new NavigationUseCase(
                _gameStateReceiver,
                monitoringEvents,
                _inMemoryMessageReceiver,
                _plannedRoute);
        }

        public static PlannedRoute FixedForTesting()
        {
            var route = new SegmentSequenceBuilder()
                .StartingAt("seg-1")
                .TuringLeftTo("seg-2")
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
        public void GivenNotStartedRouteAndRiderIsOnStartingSegment_PlannedRouteIsStarted()
        {
            _gameStateReceiver.Enqueue("segmentChanged", "seg-1");

            WhenHandlingNavigation();

            _plannedRoute
                .CurrentSegmentId
                .Should()
                .Be("seg-1");
        }

        [Fact]
        public void GivenNotStartedRouteAndRiderIsNotOnStartingSegment_CurrentSegmentRemainsEmpty()
        {
            _gameStateReceiver.Enqueue("segmentChanged", "seg-NOT-ON-SEG");

            WhenHandlingNavigation();

            _plannedRoute
                .CurrentSegmentId
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenStartedRouteAndRiderEntersNextExpectedSegment_CurrentSegmentIsUpdated()
        {
            _plannedRoute.EnteredSegment("seg-1");
            _gameStateReceiver.Enqueue("segmentChanged", "seg-2");

            WhenHandlingNavigation();

            _plannedRoute
                .CurrentSegmentId
                .Should()
                .Be("seg-2");
        }

        [Fact]
        public void GivenStartedRouteAndRiderEntersNextExpectedSegment_NextSegmentIsUpdated()
        {
            _plannedRoute.EnteredSegment("seg-1");
            _gameStateReceiver.Enqueue("segmentChanged", "seg-2");

            WhenHandlingNavigation();

            _plannedRoute
                .NextSegmentId
                .Should()
                .Be("seg-3");
        }

        [Fact]
        public void GivenStartedRouteAndRiderEntersNextExpectedSegment_ExpectedTurnIsUpdated()
        {
            _plannedRoute.EnteredSegment("seg-1");
            _gameStateReceiver.Enqueue("segmentChanged", "seg-2");

            WhenHandlingNavigation();

            _plannedRoute
                .TurnToNextSegment
                .Should()
                .Be(TurnDirection.GoStraight);
        }

        [Fact]
        public void GivenStartedRouteOnSegmentThreeAndLeftAndGoStraightCommandsAvailable_NoCommandIsSent()
        {
            _plannedRoute.EnteredSegment("seg-1");
            _plannedRoute.EnteredSegment("seg-2");
            _plannedRoute.EnteredSegment("seg-3");
            _gameStateReceiver.Enqueue("turnCommandsAvailable", new List<TurnDirection> { TurnDirection.Left , TurnDirection.GoStraight});

            WhenHandlingNavigation();

            _inMemoryMessageReceiver
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
            _gameStateReceiver.Enqueue("turnCommandsAvailable", new List<TurnDirection> { TurnDirection.Left , TurnDirection.Right});

            WhenHandlingNavigation();

            _inMemoryMessageReceiver
                .SentCommands
                .Should()
                .Contain(TurnDirection.Right.ToString())
                .And
                .HaveCount(1);
        }

        [Fact]
        public void GivenStartedRouteOnLastSegment_NextSegmentIsEmpty()
        {
            _plannedRoute.EnteredSegment("seg-1");
            _plannedRoute.EnteredSegment("seg-2");
            _plannedRoute.EnteredSegment("seg-3");
            _plannedRoute.EnteredSegment("seg-4");
            _plannedRoute.EnteredSegment("seg-5");
            _plannedRoute.EnteredSegment("seg-6");
            _plannedRoute.EnteredSegment("seg-7");

            _plannedRoute
                .NextSegmentId
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenStartedRouteOnSegmentThreeAndEnteringSegmentFive_ArgumentExceptionIsThrown()
        {
            _plannedRoute.EnteredSegment("seg-1");
            _plannedRoute.EnteredSegment("seg-2");
            _plannedRoute.EnteredSegment("seg-3");

            Action act = () => _plannedRoute.EnteredSegment("seg-5");

            act.Should()
                .Throw<ArgumentException>();

            _plannedRoute
                .NextSegmentId
                .Should()
                .Be("seg-4");
        }

        [Fact]
        public void GivenStartedRouteOnLastSegment_TurnToNextSegmentIsNone()
        {
            _plannedRoute.EnteredSegment("seg-1");
            _plannedRoute.EnteredSegment("seg-2");
            _plannedRoute.EnteredSegment("seg-3");
            _plannedRoute.EnteredSegment("seg-4");
            _plannedRoute.EnteredSegment("seg-5");
            _plannedRoute.EnteredSegment("seg-6");
            _plannedRoute.EnteredSegment("seg-7");
            
            _plannedRoute
                .TurnToNextSegment
                .Should()
                .Be(TurnDirection.None);
        }

        [Fact]
        public void GivenStartedRouteOnLastSegmentAndEnteringSegment_ArgumentExceptionIsThrown()
        {
            _plannedRoute.EnteredSegment("seg-1");
            _plannedRoute.EnteredSegment("seg-2");
            _plannedRoute.EnteredSegment("seg-3");
            _plannedRoute.EnteredSegment("seg-4");
            _plannedRoute.EnteredSegment("seg-5");
            _plannedRoute.EnteredSegment("seg-6");
            _plannedRoute.EnteredSegment("seg-7");
            
            Action act = () => _plannedRoute.EnteredSegment("seg-5");

            act
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("Route has already completed, can't enter new segment");
        }

        private void WhenHandlingNavigation()
        {
            var tokenSource = Debugger.IsAttached
                ? new CancellationTokenSource()
                : new CancellationTokenSource(50);

            try
            {
                _useCase.Execute(tokenSource.Token);
            }
            finally
            {
                tokenSource.Cancel();
            }
        }
    }
}
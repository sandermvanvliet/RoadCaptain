using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.Tests.Unit.Routing
{
    public class WhenHandlingIncomingRiderPositionMessage
    {
        private readonly HandleZwiftMessagesUseCase _useCase;
        private readonly InMemoryMessageEmitter _messageEmitter;
        private readonly FieldInfo _currentSegmentFieldInfo;
        private readonly HandleRiderPositionUseCase _handleRiderPositionUseCase;
        private readonly InMemoryGameStateDispatcher _dispatcher;

        public WhenHandlingIncomingRiderPositionMessage()
        {
            _messageEmitter = new InMemoryMessageEmitter();
            var segmentStore = new SegmentStore(@"c:\git\RoadCaptain\src\RoadCaptain.Adapters");
            segmentStore.LoadSegments();

            var monitoringEvents = new NopMonitoringEvents();

            _dispatcher = new InMemoryGameStateDispatcher(monitoringEvents);

            _handleRiderPositionUseCase = new HandleRiderPositionUseCase(
                monitoringEvents, 
                segmentStore,
                _dispatcher
            );
            _useCase = new HandleZwiftMessagesUseCase(
                _messageEmitter,
                monitoringEvents,
                new InMemoryMessageReceiver(),
                _handleRiderPositionUseCase,
                new HandleAvailableTurnsUseCase(_dispatcher),
                new HandleActivityDetailsUseCase(_dispatcher));

            _currentSegmentFieldInfo = _handleRiderPositionUseCase.GetType().GetField("_currentSegment", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [Fact]
        public void GivenPositionOnSegment_CurrentSegmentIsSet()
        {
            var gameLat = 93536.016f;
            var gameLon = 212496.77f;

            var tokenSource =Debugger.IsAttached
                ? new CancellationTokenSource()
                : new CancellationTokenSource(50);
            
            _dispatcher.EnterGame();

            GivenRiderPosition(gameLat, gameLon, 13);

            try
            {
                WhenHandlingMessage(tokenSource.Token);
            }
            finally
            {
                tokenSource.Cancel();
            }

            CurrentSegment
                .Id
                .Should()
                .Be("watopia-big-foot-hills-004-before");
        }

        [Fact]
        public void GivenRiderPositionWhenRiderNotInGame_PositionIsNotUpdated()
        {
            var gameLat = 93536.016f;
            var gameLon = 212496.77f;

            var tokenSource =Debugger.IsAttached
                ? new CancellationTokenSource()
                : new CancellationTokenSource(50);
            
            GivenRiderPosition(gameLat, gameLon, 13);

            try
            {
                WhenHandlingMessage(tokenSource.Token);
            }
            finally
            {
                tokenSource.Cancel();
            }

            CurrentSegment
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenActivityDetailsMessageWithActivityIdZero_InGameIsFalseOnDispatcher()
        {
            var tokenSource =Debugger.IsAttached
                ? new CancellationTokenSource()
                : new CancellationTokenSource(50);

            GivenActivityDetails(0);

            try
            {
                WhenHandlingMessage(tokenSource.Token);
            }
            finally
            {
                tokenSource.Cancel();
            }

            _dispatcher.InGame
                .Should()
                .BeFalse();
        }

        [Fact]
        public void GivenActivityDetailsMessageWithActivityIdNonZero_InGameIsFalseOnDispatcher()
        {
            var tokenSource =Debugger.IsAttached
                ? new CancellationTokenSource()
                : new CancellationTokenSource(50);

            GivenActivityDetails(123);

            try
            {
                WhenHandlingMessage(tokenSource.Token);
            }
            finally
            {
                tokenSource.Cancel();
            }

            _dispatcher.InGame
                .Should()
                .BeTrue();
        }

        [Fact]
        public void GivenRiderLeavesGame_CurrentSegmentIsCleared()
        {
            GivenRiderInGameAndOnSegment();
            
            var tokenSource =Debugger.IsAttached
                ? new CancellationTokenSource()
                : new CancellationTokenSource(50);

            GivenActivityDetails(0);

            try
            {
                WhenHandlingMessage(tokenSource.Token);
            }
            finally
            {
                tokenSource.Cancel();
            }

            _dispatcher.CurrentSegment
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenRiderLeavesGame_CurrentDirectionIsSetToUnknown()
        {
            GivenRiderInGameAndOnSegment();
            
            var tokenSource =Debugger.IsAttached
                ? new CancellationTokenSource()
                : new CancellationTokenSource(50);

            GivenActivityDetails(0);

            try
            {
                WhenHandlingMessage(tokenSource.Token);
            }
            finally
            {
                tokenSource.Cancel();
            }

            _dispatcher.CurrentDirection
                .Should()
                .Be(SegmentDirection.Unknown);
        }

        [Fact]
        public void GivenRiderLeavesGame_AvailableTurnsAreCleared()
        {
            GivenRiderInGameAndOnSegment();
            
            var tokenSource =Debugger.IsAttached
                ? new CancellationTokenSource()
                : new CancellationTokenSource(50);

            GivenActivityDetails(0);

            try
            {
                WhenHandlingMessage(tokenSource.Token);
            }
            finally
            {
                tokenSource.Cancel();
            }

            _dispatcher.AvailableTurnCommands
                .Should()
                .BeEmpty();
        }


        private void WhenHandlingMessage(CancellationToken token)
        {
            _useCase.Execute(token);
        }

        private void GivenRiderPosition(float x, float y, float altitude)
        {
            _messageEmitter
                .Enqueue(new ZwiftRiderPositionMessage
                {
                    Latitude = x,
                    Longitude = y,
                    Altitude = altitude
                });
        }

        private void GivenActivityDetails(ulong activityId)
        {
            _messageEmitter.Enqueue(new ZwiftActivityDetailsMessage { ActivityId = activityId });
        }

        private Segment CurrentSegment
        {
            get
            {
                var value = _currentSegmentFieldInfo.GetValue(_handleRiderPositionUseCase);
                return value as Segment;
            }
        }

        private void GivenRiderInGameAndOnSegment()
        {
            _dispatcher.EnterGame();

            const float gameLat = 93536.016f;
            const float gameLon = 212496.77f;
            GivenRiderPosition(gameLat, gameLon, 13);

            var tokenSource =Debugger.IsAttached
                ? new CancellationTokenSource()
                : new CancellationTokenSource(50);
            try
            {
                WhenHandlingMessage(tokenSource.Token);
            }
            finally
            {
                tokenSource.Cancel();
            }

            _dispatcher.DirectionChanged(SegmentDirection.AtoB);
            _dispatcher.TurnCommandsAvailable(new List<TurnDirection> { TurnDirection.Left, TurnDirection.Right });
        }
    }
}

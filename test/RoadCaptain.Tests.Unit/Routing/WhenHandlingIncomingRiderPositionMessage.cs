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

        public WhenHandlingIncomingRiderPositionMessage()
        {
            _messageEmitter = new InMemoryMessageEmitter();
            var segmentStore = new SegmentStore(@"c:\git\RoadCaptain\src\RoadCaptain.Adapters");
            segmentStore.LoadSegments();

            _useCase = new HandleZwiftMessagesUseCase(
                _messageEmitter,
                new NopMonitoringEvents(),
                new InMemoryMessageReceiver(),
                segmentStore);

            _currentSegmentFieldInfo = _useCase.GetType().GetField("_currentSegment", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [Fact]
        public void GivenPositionOnSegment_CurrentSegmentIsSet()
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
                .Id
                .Should()
                .Be("watopia-big-foot-hills-004-before");
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

        private Segment CurrentSegment
        {
            get
            {
                var value = _currentSegmentFieldInfo.GetValue(_useCase);
                return value as Segment;
            }
        }
    }
}

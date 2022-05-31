using System.Linq;
using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class WhenCompletedRouteStateIsReceived : EngineTest
    {
        [Fact]
        public void GivenRouteCompletesAndUserPreferenceIsToEndActivity_EndActivityCommandIsSent()
        {
            UserPreferences.EndActivityAtEndOfRoute = true;

            GivenCompletedRouteStateReceived();

            SentCommands
                .Should()
                .Contain($"ENDACTIVITY;RoadCaptain: {_route.Name}");
        }

        [Fact]
        public void GivenRouteCompletesAndUserPreferenceIsToContinueActivity_EndActivityCommandIsNotSent()
        {
            UserPreferences.EndActivityAtEndOfRoute = false;

            GivenCompletedRouteStateReceived();

            SentCommands
                .Should()
                .NotContain($"ENDACTIVITY;RoadCaptain: {_route.Name}");
        }

        [Fact]
        public void GivenLoopedRouteAndUserWantsToLoop_RouteIsResetToNotStarted()
        {
            UserPreferences.LoopRouteAtEndOfRoute = true;

            _route = new SegmentSequenceBuilder()
                .StartingAt("seg-1")
                .GoingStraightTo("seg-2")
                .GoingStraightTo("seg-3")
                .EndingAt("seg-3")
                .Build();

            _route.RouteSegmentSequence.Last().NextSegmentId = "seg-1";

            _route.EnteredSegment("seg-1");
            _route.EnteredSegment("seg-2");
            _route.EnteredSegment("seg-3");

            GivenCompletedRouteStateReceived();

            _route.SegmentSequenceIndex.Should().Be(0);
        }

        private PlannedRoute _route = new PlannedRoute { Name = "Test Route" };

        private void GivenCompletedRouteStateReceived()
        {
            ReceiveGameState(new CompletedRouteState(1, 2, new TrackPoint(1,2,3), _route));
        }
    }
}
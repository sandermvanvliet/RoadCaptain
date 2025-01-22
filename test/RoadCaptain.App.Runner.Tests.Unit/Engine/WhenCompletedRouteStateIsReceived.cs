// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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

        private PlannedRoute _route = new PlannedRoute { Name = "Test Route" };

        private void GivenCompletedRouteStateReceived()
        {
            ReceiveGameState(new CompletedRouteState(1, 2, new TrackPoint(1,2,3), _route));
        }
    }
}

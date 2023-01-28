// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using FluentAssertions;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class WhenReadyToGoStateIsReceived : EngineTest
    {

        [Fact]
        public void ZwiftConnectionListenerIsStarted()
        {
            GivenReadyToGoStateIsReceived();

            TheTaskWithName("_listenerTask")
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void ZwiftConnectionInitiatorIsStarted()
        {
            GivenReadyToGoStateIsReceived();

            TheTaskWithName("_initiatorTask")
                .Should()
                .NotBeNull();
        }

        private void GivenReadyToGoStateIsReceived()
        {
            ReceiveGameState(new ReadyToGoState());
        }
    }
}

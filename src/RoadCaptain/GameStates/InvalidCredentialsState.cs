// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;

namespace RoadCaptain.GameStates
{
    public sealed class InvalidCredentialsState : GameState
    {
        public Exception Exception { get; }

        public InvalidCredentialsState(Exception exception)
        {
            Exception = exception;
        }

        public override uint RiderId => 0;
        public override GameState EnterGame(uint riderId, ulong activityId)
        {
            throw InvalidStateTransitionException.NotLoggedIn(GetType());
        }

        public override GameState LeaveGame()
        {
            throw InvalidStateTransitionException.NotLoggedIn(GetType());
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            throw InvalidStateTransitionException.NotInGame(GetType());
        }

        public override GameState TurnCommandAvailable(string type)
        {
            throw InvalidStateTransitionException.NotOnARoute(GetType());
        }
    }
}

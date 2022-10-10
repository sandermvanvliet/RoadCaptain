// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;

namespace RoadCaptain.GameStates
{
    public sealed class NotLoggedInState : GameState
    {
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

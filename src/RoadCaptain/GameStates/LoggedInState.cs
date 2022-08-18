using System;
using System.Collections.Generic;

namespace RoadCaptain.GameStates
{
    public sealed class LoggedInState : GameState
    {
        public string AccessToken { get; }

        public LoggedInState(string accessToken)
        {
            AccessToken = accessToken;
        }
        public override uint RiderId => 0;
        public override GameState EnterGame(uint riderId, ulong activityId)
        {
            return new InGameState(riderId, activityId);
        }

        public override GameState LeaveGame()
        {
            return new ConnectedToZwiftState();
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            return this;
        }

        public override GameState TurnCommandAvailable(string type)
        {
            return this;
        }
    }
}

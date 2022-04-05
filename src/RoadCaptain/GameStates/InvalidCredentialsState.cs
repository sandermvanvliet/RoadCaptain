using System;
using System.Collections.Generic;

namespace RoadCaptain.GameStates
{
    public class InvalidCredentialsState : GameState
    {
        public Exception Exception { get; }

        public InvalidCredentialsState(Exception exception)
        {
            Exception = exception;
        }

        public override uint RiderId => 0;
        public override GameState EnterGame(uint riderId, ulong activityId)
        {
            throw new System.NotImplementedException();
        }

        public override GameState LeaveGame()
        {
            throw new System.NotImplementedException();
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            throw new System.NotImplementedException();
        }

        public override GameState TurnCommandAvailable(string type)
        {
            throw new System.NotImplementedException();
        }
    }
}
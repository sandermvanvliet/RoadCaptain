using System;
using System.Collections.Generic;

namespace RoadCaptain.GameStates
{
    public class LoggedInState : GameState
    {
        public override uint RiderId => 0;
        public override GameState EnterGame(uint riderId, ulong activityId)
        {
            throw new NotImplementedException();
        }

        public override GameState LeaveGame()
        {
            throw new NotImplementedException();
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            throw new NotImplementedException();
        }

        public override GameState TurnCommandAvailable(string type)
        {
            throw new NotImplementedException();
        }
    }
}

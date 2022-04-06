using System.Collections.Generic;

namespace RoadCaptain.GameStates
{
    public class WaitingForConnectionState : GameState
    {
        public override uint RiderId => 0;

        public override GameState EnterGame(uint riderId, ulong activityId)
        {
            return this;
        }

        public override GameState LeaveGame()
        {
            return this;
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

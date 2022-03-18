using System.Collections.Generic;

namespace RoadCaptain.GameStates
{
    public class NotInGameState : GameState
    {
        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            return this;
        }

        public sealed override uint RiderId => 0;

        public sealed override GameState EnterGame(uint riderId, ulong activityId)
        {
            return new InGameState(riderId, activityId);
        }

        public override GameState LeaveGame()
        {
            return this;
        }

        public override GameState TurnCommandAvailable(string type)
        {
            return this;
        }
    }
}
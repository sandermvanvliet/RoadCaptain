using System.Collections.Generic;

namespace RoadCaptain.GameStates
{
    public class NotInGameState : GameState
    {
        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            return this;
        }

        public override GameState EnterGame(ulong activityId)
        {
            return new InGameState(activityId);
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
using System.Collections.Generic;

namespace RoadCaptain.GameState
{
    public class NotInGameState : GameState
    {
        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments)
        {
            return new NotInGameState();
        }

        public override GameState EnterGame(int activityId)
        {
            return new InGameState(activityId);
        }

        public override GameState EnterSegment()
        {
            return new NotInGameState();
        }

        public override GameState LeaveGame()
        {
            return new NotInGameState();
        }
    }
}
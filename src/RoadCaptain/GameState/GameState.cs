using System.Collections.Generic;

namespace RoadCaptain.GameState
{
    public abstract class GameState
    {
        public abstract GameState UpdatePosition(TrackPoint position, List<Segment> segments);
        public abstract GameState EnterGame(int activityId);
        public abstract GameState EnterSegment();
        public abstract GameState LeaveGame();
    }
}
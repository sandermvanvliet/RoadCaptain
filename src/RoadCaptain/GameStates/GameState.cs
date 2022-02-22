using System.Collections.Generic;

namespace RoadCaptain.GameStates
{
    public abstract class GameState
    {
        public abstract GameState EnterGame(ulong activityId);
        public abstract GameState LeaveGame();
        public abstract GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute);
        public abstract GameState TurnCommandAvailable(string type);
    }
}
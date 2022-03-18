using System.Collections.Generic;

namespace RoadCaptain.GameStates
{
    public abstract class GameState
    {
        public abstract uint RiderId { get;  }
        public abstract GameState EnterGame(uint riderId, ulong activityId);
        public abstract GameState LeaveGame();
        public abstract GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute);
        public abstract GameState TurnCommandAvailable(string type);
    }
}
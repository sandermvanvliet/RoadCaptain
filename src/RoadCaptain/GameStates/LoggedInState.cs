using System.Collections.Generic;

namespace RoadCaptain.GameStates
{
    public sealed class LoggedInState : GameState
    {
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
            throw InvalidStateTransitionException.NotInGame(GetType());
        }

        public override GameState TurnCommandAvailable(string type)
        {
            throw InvalidStateTransitionException.NotOnARoute(GetType());
        }
    }
}

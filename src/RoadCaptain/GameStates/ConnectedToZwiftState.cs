using System.Collections.Generic;

namespace RoadCaptain.GameStates
{
    public sealed class ConnectedToZwiftState : GameState
    {
        // TODO: Set RiderId because we should already know this when we're connected to Zwift
        public override uint RiderId => 0;

        public override GameState EnterGame(uint riderId, ulong activityId)
        {
            return new InGameState(riderId, activityId);
        }

        public override GameState LeaveGame()
        {
            throw InvalidStateTransitionException.NotLoggedIn(GetType());
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
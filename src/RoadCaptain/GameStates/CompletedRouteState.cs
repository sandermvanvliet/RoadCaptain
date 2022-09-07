using System.Collections.Generic;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public sealed class CompletedRouteState : GameState
    {
        public CompletedRouteState(
            uint riderId, 
            ulong activityId, 
            TrackPoint currentPosition,
            PlannedRoute plannedRoute)
        {
            RiderId = riderId;
            ActivityId = activityId;
            CurrentPosition = currentPosition;
            Route = plannedRoute;
            Route.Complete();
        }

        [JsonProperty]
        public override uint RiderId { get; }

        [JsonProperty]
        public ulong ActivityId { get; }
        
        [JsonProperty]
        public TrackPoint CurrentPosition { get; }

        [JsonProperty]
        public PlannedRoute Route { get; private set; }
        
        public override GameState EnterGame(uint riderId, ulong activityId)
        {
            // There are cases where Zwift sends this for an ongoing activity
            // so there we remain in the same state.
            if (RiderId == riderId && ActivityId == activityId)
            {
                return this;
            }

            throw InvalidStateTransitionException.AlreadyInGame(GetType());
        }

        public override GameState LeaveGame()
        {
            return new ConnectedToZwiftState();
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            return new CompletedRouteState(RiderId, ActivityId, position, plannedRoute);
        }

        public override GameState TurnCommandAvailable(string type)
        {
            return this;
        }
    }
}
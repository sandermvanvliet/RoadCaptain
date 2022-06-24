using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class CompletedRouteState : PositionedState
    {
        public CompletedRouteState(uint riderId, ulong activityId, TrackPoint currentPosition,
            PlannedRoute plannedRoute) : base(riderId, activityId, currentPosition)
        {
            Route = plannedRoute;
            Route.Complete();
        }

        [JsonProperty]
        public PlannedRoute Route { get; private set; }
    }
}
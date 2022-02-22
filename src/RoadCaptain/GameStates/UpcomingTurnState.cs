using System.Collections.Generic;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class UpcomingTurnState : OnRouteState
    {
        public UpcomingTurnState(
            ulong activityId,
            TrackPoint currentPosition,
            Segment segment,
            PlannedRoute plannedRoute,
            SegmentDirection direction,
            List<TurnDirection> directions)
            : base(activityId, currentPosition, segment, plannedRoute, direction)
        {
            Directions = directions;
        }

        [JsonProperty]
        public List<TurnDirection> Directions { get; private set; }
    }
}
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

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            var result = base.UpdatePosition(position, segments, plannedRoute);

            if (result is OnRouteState routeState)
            {
                if (routeState.CurrentSegment.Id == CurrentSegment.Id)
                {
                    // We're still on the same segment
                    return this;
                }
            }

            return result;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class InGameState : GameState
    {
        [JsonProperty]
        public ulong ActivityId { get; private set; }

        [JsonProperty]
        public sealed override uint RiderId { get; }

        public InGameState(uint riderId, ulong activityId)
        {
            RiderId = riderId;
            ActivityId = activityId;
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            // Note: We're using an IEnumerable<T> here to prevent
            //       unnecessary ToList() calls because the foreach
            //       loop in GetClosestMatchingSegment handles that
            //       for us.
            var matchingSegments = segments.Where(s => s.Contains(position));
            
            var (segment, closestOnSegment) = matchingSegments.GetClosestMatchingSegment(position, TrackPoint.Unknown);

            if (segment == null)
            {
                return new PositionedState(RiderId, ActivityId, position);
            }

            // This is to ensure that we have the segment of the position
            // for future reference.
            closestOnSegment.Segment = segment;

            if (!plannedRoute.HasStarted && plannedRoute.StartingSegmentId == segment.Id)
            {
                plannedRoute.EnteredSegment(segment.Id);
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute);
            }

            if (plannedRoute.HasStarted && !plannedRoute.HasCompleted && plannedRoute.CurrentSegmentId == segment.Id)
            {
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute);
            }
            
            if (plannedRoute.HasStarted && plannedRoute.NextSegmentId == segment.Id)
            {
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute);
            }

            return new OnSegmentState(RiderId, ActivityId, closestOnSegment, segment, SegmentDirection.Unknown, 0, 0, 0);
        }

        public sealed override GameState EnterGame(uint riderId, ulong activityId)
        {
            if (RiderId == riderId && ActivityId == activityId)
            {
                return this;
            }

            return new InGameState(riderId, activityId);
        }

        public sealed override GameState LeaveGame()
        {
            return new ConnectedToZwiftState();
        }

        public override GameState TurnCommandAvailable(string type)
        {
            return this;
        }
    }
}
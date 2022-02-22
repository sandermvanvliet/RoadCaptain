using System.Collections.Generic;
using System.Linq;

namespace RoadCaptain.GameStates
{
    public class OnRouteState : OnSegmentState
    {
        public PlannedRoute Route { get; }

        public OnRouteState(int activityId, TrackPoint currentPosition, Segment segment, PlannedRoute plannedRoute) 
            : base(activityId, currentPosition, segment)
        {
            Route = plannedRoute;
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            /*
             * Next steps:
             * - Determine which segment we're on based on the position
             * - Determine direction on that segment (to which end of the segment are we moving)
             * - Determine next turn action (left/straight/right)
             */
            var matchingSegments = segments
                .Where(s => s.Contains(position))
                .ToList();

            if (!matchingSegments.Any())
            {
                return new PositionedState(ActivityId, position);
            }

            var (segment, closestOnSegment) = GetClosestMatchingSegment(matchingSegments, position);

            if (segment.Id == Route.NextSegmentId)
            {
                Route.EnteredSegment(segment.Id);

                return new OnRouteState(ActivityId, position, segment, Route);
            }

            return new OnSegmentState(ActivityId, position, segment);
        }
    }
}
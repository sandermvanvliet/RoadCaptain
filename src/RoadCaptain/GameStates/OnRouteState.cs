using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class OnRouteState : OnSegmentState
    {
        public OnRouteState(ulong activityId, TrackPoint currentPosition, Segment segment, PlannedRoute plannedRoute)
            : base(activityId, currentPosition, segment)
        {
            Route = plannedRoute;
        }

        [JsonProperty]
        public PlannedRoute Route { get; private set; }

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
                try
                {
                    Route.EnteredSegment(segment.Id);
                }
                catch (ArgumentException e)
                {
                    // The segment is not the expected next one so we lost lock somewhere...
                }

                return new OnRouteState(ActivityId, position, segment, Route);
            }

            if (segment.Id == Route.CurrentSegmentId)
            {
                return new OnRouteState(ActivityId, position, segment, Route);
            }

            var distance = position.DistanceTo(CurrentPosition);

            if (distance < 100)
            {
                return new OnRouteState(
                    ActivityId,
                    CurrentPosition, // Use the last known position on the segment
                    CurrentSegment, // Use the current segment of the route
                    Route);
            }

            return new OnSegmentState(ActivityId, position, segment);
        }
    }
}
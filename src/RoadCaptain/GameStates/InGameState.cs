using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class InGameState : GameState
    {
        [JsonProperty]
        public ulong ActivityId { get; private set; }

        public InGameState(ulong activityId)
        {
            ActivityId = activityId;
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

            if (!plannedRoute.HasStarted && plannedRoute.StartingSegmentId == segment.Id)
            {
                plannedRoute.EnteredSegment(segment.Id);
                return new OnRouteState(ActivityId, closestOnSegment, segment, plannedRoute);
            }

            return new OnSegmentState(ActivityId, closestOnSegment, segment);
        }

        public override GameState EnterGame(ulong activityId)
        {
            if (ActivityId == activityId)
            {
                return this;
            }

            return new InGameState(activityId);
        }

        public override GameState LeaveGame()
        {
            return new NotInGameState();
        }

        public override GameState TurnCommandAvailable(string type)
        {
            return this;
        }

        protected static (Segment,TrackPoint) GetClosestMatchingSegment(List<Segment> segments, TrackPoint position)
        {
            // If there is only one segment then we can
            // do a quick exist and not bother to figure
            // out which point is actually closest.
            if (segments.Count == 1)
            {
                return (segments[0], segments[0].GetClosestPositionOnSegment(position));
            }

            // For each segment find the closest track point in that segment
            // in relation to the current position
            TrackPoint closest = null;
            decimal? closestDistance = null;
            Segment closestSegment = null;

            foreach (var segment in segments)
            {
                // This is very suboptimal as this needs to traverse
                // all the points of the segment whereas finding if
                // the point is on the segment can stop at the first
                // hit.
                var closestOnSegment = segment
                    .Points
                    .Select(p => new { Point = p, Distance = p.DistanceTo(position) })
                    .OrderBy(d => d.Distance)
                    .First();

                if (closest == null)
                {
                    closest = closestOnSegment.Point;
                    closestDistance = closestOnSegment.Distance;
                    closestSegment = segment;
                }
                else if (closestOnSegment.Distance < closestDistance)
                {
                    closest = closestOnSegment.Point;
                    closestDistance = closestOnSegment.Distance;
                    closestSegment = segment;
                }
            }

            return (closestSegment, closest);
        }
    }
}
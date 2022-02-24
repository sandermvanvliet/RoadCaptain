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
            // Note: We're using an IEnumerable<T> here to prevent
            //       unnecessary ToList() calls because the foreach
            //       loop in GetClosestMatchingSegment handles that
            //       for us.
            var matchingSegments = segments.Where(s => s.Contains(position));

            var (segment, closestOnSegment) = GetClosestMatchingSegment(matchingSegments, position);

            if (segment == null)
            {
                return new PositionedState(ActivityId, position);
            }

            if (!plannedRoute.HasStarted && plannedRoute.StartingSegmentId == segment.Id)
            {
                plannedRoute.EnteredSegment(segment.Id);
                return new OnRouteState(ActivityId, closestOnSegment, segment, plannedRoute);
            }

            if (plannedRoute.HasStarted && plannedRoute.CurrentSegmentId == segment.Id)
            {
                return new OnRouteState(ActivityId, closestOnSegment, segment, plannedRoute);
            }
            
            if (plannedRoute.HasStarted && plannedRoute.NextSegmentId == segment.Id)
            {
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

        private static (Segment,TrackPoint) GetClosestMatchingSegment(IEnumerable<Segment> segments, TrackPoint position)
        {
            // For each segment find the closest track point in that segment
            // in relation to the current position
            TrackPoint closestPoint = null;
            double? distanceToClosestPoint = null;
            Segment closestSegment = null;

            foreach (var segment in segments)
            {
                // This is very suboptimal as this needs to traverse
                // all the points of the segment whereas finding if
                // the point is on the segment can stop at the first
                // hit.
                // The optimization here is to at least exclude points
                // which we know are too far away using IsCloseToQuick()
                // however that still enumerates all points in the 
                // segment.
                var closestOnSegment = segment
                    .Points
                    .Where(p => TrackPoint.IsCloseToQuick(p.Longitude, position))
                    .Select(p => new { Point = p, Distance = p.DistanceTo(position)})                         
                    .OrderBy(d => d.Distance)
                    .First();

                if (closestPoint == null)
                {
                    closestPoint = closestOnSegment.Point;
                    distanceToClosestPoint = closestOnSegment.Distance;
                    closestSegment = segment;
                }
                else if (closestOnSegment.Distance < distanceToClosestPoint)
                {
                    closestPoint = closestOnSegment.Point;
                    distanceToClosestPoint = closestOnSegment.Distance;
                    closestSegment = segment;
                }
            }

            return (closestSegment, closestPoint);
        }
    }
}
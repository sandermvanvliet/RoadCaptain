using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class PositionedState : InGameState
    {
        [JsonProperty]
        public TrackPoint CurrentPosition { get; private set; }

        public PositionedState(uint riderId, ulong activityId, TrackPoint currentPosition)
            : base(riderId, activityId)
        {
            CurrentPosition = currentPosition;
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            // Note: We're using an IEnumerable<T> here to prevent
            //       unnecessary ToList() calls because the foreach
            //       loop in GetClosestMatchingSegment handles that
            //       for us.
            var matchingSegments = segments.Where(s => s.Contains(position));
            
            var (segment, closestOnSegment) = GetClosestMatchingSegment(matchingSegments, position, CurrentPosition);
            
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

            return new OnSegmentState(RiderId, ActivityId, closestOnSegment, segment);
        }

        private static (Segment, TrackPoint) GetClosestMatchingSegment(IEnumerable<Segment> segments,
            TrackPoint position, TrackPoint currentPosition)
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
                else if (closestOnSegment.Distance < distanceToClosestPoint && Math.Abs(closestOnSegment.Point.Altitude - currentPosition.Altitude) < 2)
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
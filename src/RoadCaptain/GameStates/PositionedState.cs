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

        protected override (Segment, TrackPoint) GetClosestMatchingSegment(IEnumerable<Segment> segments, TrackPoint position)
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
                // This method is called from PositionedState where there _is_ a current position
                // to check the altitude against for segment overlaps. Because InGameState doesn't
                // have a position at all we have the null check here to deal with that situation
                // as I really don't want to duplicate this code.
                else if (closestOnSegment.Distance < distanceToClosestPoint &&
                         Math.Abs(closestOnSegment.Point.Altitude - CurrentPosition.Altitude) < 2)
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
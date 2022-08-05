using System.Collections.Generic;

namespace RoadCaptain.GameStates
{
    public class LostRouteLockState : OnSegmentState
    {
        public PlannedRoute PlannedRoute { get; }
        
        public LostRouteLockState(uint riderId, ulong activityId, TrackPoint currentPosition, Segment segment,
            SegmentDirection direction, PlannedRoute plannedRoute, double elapsedDistance, double elapsedAscent, double elapsedDescent) 
            : base(riderId, activityId, currentPosition, segment, direction, elapsedDistance, elapsedAscent, elapsedDescent)
        {
            PlannedRoute = plannedRoute;
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            var result = base.UpdatePosition(position, segments, plannedRoute);

            return result;
        }
    }
}
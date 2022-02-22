using System;
using System.Collections.Generic;
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

        private OnRouteState(ulong activityId, TrackPoint currentPosition, Segment segment, PlannedRoute plannedRoute, SegmentDirection direction) 
            : base(activityId, currentPosition, segment, direction)
        {
            Route = plannedRoute;
        }

        [JsonProperty]
        public PlannedRoute Route { get; private set; }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            var result = base.UpdatePosition(position, segments, plannedRoute);

            if (result is OnSegmentState segmentState)
            {
                if (segmentState.CurrentSegment.Id == Route.NextSegmentId)
                {
                    try
                    {
                        Route.EnteredSegment(segmentState.CurrentSegment.Id);
                    }
                    catch (ArgumentException)
                    {
                        // The segment is not the expected next one so we lost lock somewhere...
                    }

                    return new OnRouteState(ActivityId, position, segmentState.CurrentSegment, Route);
                }

                if (segmentState.CurrentSegment.Id == Route.CurrentSegmentId)
                {
                    return new OnRouteState(ActivityId, position, segmentState.CurrentSegment, Route, segmentState.Direction);
                }

                var distance = position.DistanceTo(CurrentPosition);

                if (distance < 100)
                {
                    return new OnRouteState(
                        ActivityId,
                        CurrentPosition, // Use the last known position on the segment
                        CurrentSegment, // Use the current segment of the route
                        Route,
                        Direction);
                }

                return segmentState;
            }

            return result;
        }
    }
}
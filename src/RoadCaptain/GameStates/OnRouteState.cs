using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class OnRouteState : OnSegmentState
    {
        public OnRouteState(uint riderId, ulong activityId, TrackPoint currentPosition, Segment segment,
            PlannedRoute plannedRoute)
            : base(riderId, activityId, currentPosition, segment)
        {
            Route = plannedRoute;
        }

        protected OnRouteState(uint riderId, ulong activityId, TrackPoint currentPosition, Segment segment,
            PlannedRoute plannedRoute, SegmentDirection direction)
            : base(riderId, activityId, currentPosition, segment, direction)
        {
            Route = plannedRoute;
        }

        protected OnRouteState(uint riderId, ulong activityId, TrackPoint currentPosition, Segment segment,
            PlannedRoute plannedRoute,
            SegmentDirection direction, List<TurnDirection> turnDirections) 
            : this(riderId, activityId, currentPosition, segment, plannedRoute, direction)
        {
            TurnCommands = turnDirections;
        }

        [JsonProperty]
        public PlannedRoute Route { get; private set; }

        private List<TurnDirection> TurnCommands { get; } = new();

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

                    return new OnRouteState(RiderId, ActivityId, segmentState.CurrentPosition, segmentState.CurrentSegment, Route);
                }

                if (segmentState.CurrentSegment.Id == Route.CurrentSegmentId)
                {
                    return new OnRouteState(RiderId, ActivityId, segmentState.CurrentPosition, segmentState.CurrentSegment, Route, segmentState.Direction);
                }

                var distance = position.DistanceTo(CurrentPosition);

                if (distance < 100)
                {
                    return new OnRouteState(
                        RiderId, 
                        ActivityId, 
                        CurrentPosition, // Use the last known position on the segment
                        CurrentSegment,  // Use the current segment of the route
                        Route, 
                        Direction);
                }

                return segmentState;
            }

            return result;
        }

        public override GameState TurnCommandAvailable(string type)
        {
            var turnDirection = GetTurnDirectionFor(type);

            if (turnDirection == TurnDirection.None)
            {
                return this;
            }

            if (Direction == SegmentDirection.Unknown)
            {
                // As long as we don't know the direction we're 
                // heading in the turn directions won't make any
                // sense.
                return this;
            }

            if (!TurnCommands.Contains(turnDirection))
            {
                var x = new List<TurnDirection>{ turnDirection};
                x.AddRange(TurnCommands);

                if (x.Count == CurrentSegment.NextSegments(Direction).Count &&
                    x.Count != 1) // If there is only 1 command then it means there are two segments joining without any intersection
                {
                    // We've got all the turn commands for this segment
                    return new UpcomingTurnState(RiderId, ActivityId, CurrentPosition, CurrentSegment, Route, Direction, x);
                }

                // Add the new list of turn directions to
                // a new state.
                return new OnRouteState(RiderId, ActivityId, CurrentPosition, CurrentSegment, Route, Direction, x);
            }

            return this;
        }

        private static TurnDirection GetTurnDirectionFor(string type)
        {
            return type.Trim().ToLower() switch
            {
                "turnleft" => TurnDirection.Left,
                "turnright" => TurnDirection.Right,
                "gostraight" => TurnDirection.GoStraight,
                _ => TurnDirection.None
            };
        }
    }
}
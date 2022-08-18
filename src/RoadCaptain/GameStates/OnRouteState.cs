using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public sealed class OnRouteState : GameState
    {
        public OnRouteState(uint riderId, ulong activityId, TrackPoint currentPosition, Segment segment,
            PlannedRoute plannedRoute, SegmentDirection direction, double elapsedDistance, double elapsedAscent, double elapsedDescent)
            
        {
            RiderId = riderId;
            ActivityId = activityId;
            CurrentPosition = currentPosition;
            CurrentSegment = segment;
            Route = plannedRoute;
            Direction = direction;
            ElapsedDistance = elapsedDistance;
            ElapsedAscent = elapsedAscent;
            ElapsedDescent = elapsedDescent;
        }

        private OnRouteState(uint riderId, ulong activityId, TrackPoint currentPosition, Segment segment,
            PlannedRoute plannedRoute,
            SegmentDirection direction, List<TurnDirection> turnDirections, double elapsedDistance, double elapsedAscent, double elapsedDescent) 
            : this(riderId, activityId, currentPosition, segment, plannedRoute, direction, elapsedDistance, elapsedAscent, elapsedDescent)
        {
            TurnCommands = turnDirections;
        }

        [JsonProperty]
        public sealed override uint RiderId { get; }

        [JsonProperty]
        public ulong ActivityId { get; }
        
        [JsonProperty]
        public TrackPoint CurrentPosition { get; }

        [JsonProperty]
        public Segment CurrentSegment { get; }

        [JsonProperty]
        public SegmentDirection Direction { get; private set; }

        public double ElapsedDistance { get; }

        public double ElapsedDescent { get; }

        public double ElapsedAscent { get; }

        [JsonProperty]
        public PlannedRoute Route { get; private set; }

        private List<TurnDirection> TurnCommands { get; } = new();
        
        public override GameState EnterGame(uint riderId, ulong activityId)
        {
            throw InvalidStateTransitionException.AlreadyInGame(GetType());
        }

        public override GameState LeaveGame()
        {
            return new ConnectedToZwiftState();
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            // Note: We're using an IEnumerable<T> here to prevent
            //       unnecessary ToList() calls because the foreach
            //       loop in GetClosestMatchingSegment handles that
            //       for us.
            var matchingSegments = segments.Where(s => s.Contains(position));
            
            var (segment, closestOnSegment) = matchingSegments.GetClosestMatchingSegment(position, CurrentPosition);

            if (segment == null || closestOnSegment == null)
            {
                return new PositionedState(RiderId, ActivityId, position);
            }

            var positionDelta = CurrentPosition.DeltaTo(closestOnSegment);

            var distance1 = ElapsedDistance + positionDelta.Distance;
            var ascent = ElapsedAscent + positionDelta.Ascent;
            var descent = ElapsedDescent + positionDelta.Descent;
            var direction = DetermineSegmentDirection(segment, closestOnSegment);

            if (!plannedRoute.HasStarted && plannedRoute.StartingSegmentId == segment.Id)
            {
                plannedRoute.EnteredSegment(segment.Id);
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction, distance1, ascent, descent);
            }

            if (plannedRoute.HasStarted && !plannedRoute.HasCompleted && plannedRoute.CurrentSegmentId == segment.Id)
            {
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction, distance1, ascent, descent);
            }

            if (plannedRoute.HasStarted && plannedRoute.NextSegmentId == segment.Id)
            {
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction, distance1, ascent, descent);
            }

            var segmentState = new OnSegmentState(RiderId, ActivityId, closestOnSegment, segment, direction, distance1, ascent, descent);

            if (plannedRoute.IsOnLastSegment)
            {
                if (plannedRoute.CurrentSegmentId != segmentState.CurrentSegment.Id)
                {
                    var lastOfRoute = plannedRoute.RouteSegmentSequence[plannedRoute.SegmentSequenceIndex];
                    var lastSegmentOfRoute = segments.Single(s => s.Id == plannedRoute.CurrentSegmentId);
                    if (lastOfRoute.Direction == SegmentDirection.AtoB)
                    {
                        if (lastSegmentOfRoute.NextSegmentsNodeB.Any(t =>
                                t.SegmentId == segmentState.CurrentSegment.Id))
                        {
                            // Moved from last segment of route to the next segment at the end of that
                            return new CompletedRouteState(RiderId, ActivityId, segmentState.CurrentPosition,
                                plannedRoute);
                        }

                        // TODO reproduce this with the ItalianVillasRepro route
                        return new LostRouteLockState(RiderId, ActivityId, segmentState.CurrentPosition,
                            segmentState.CurrentSegment, plannedRoute,
                            segmentState.Direction, segmentState.ElapsedDistance, segmentState.ElapsedAscent, segmentState.ElapsedDescent);
                    }

                    if (lastOfRoute.Direction == SegmentDirection.BtoA)
                    {
                        if (lastSegmentOfRoute.NextSegmentsNodeA.Any(t =>
                                t.SegmentId == segmentState.CurrentSegment.Id))
                        {
                            // Moved from last segment of route to the next segment at the end of that
                            return new CompletedRouteState(RiderId, ActivityId, segmentState.CurrentPosition,
                                plannedRoute);
                        }

                        return new LostRouteLockState(RiderId, ActivityId, segmentState.CurrentPosition,
                            segmentState.CurrentSegment, plannedRoute,
                            segmentState.Direction, segmentState.ElapsedDistance, segmentState.ElapsedAscent, segmentState.ElapsedDescent);
                    }
                }

                return new OnRouteState(RiderId, ActivityId, segmentState.CurrentPosition,
                    segmentState.CurrentSegment, Route, segmentState.Direction, segmentState.ElapsedDistance,
                    segmentState.ElapsedAscent, segmentState.ElapsedDescent);
            }

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

                return new OnRouteState(RiderId, ActivityId, segmentState.CurrentPosition, segmentState.CurrentSegment,
                    Route, direction, distance1, ascent, descent);
            }

            if (segmentState.CurrentSegment.Id == Route.CurrentSegmentId)
            {
                return new OnRouteState(RiderId, ActivityId, segmentState.CurrentPosition, segmentState.CurrentSegment,
                    Route, segmentState.Direction, segmentState.ElapsedDistance, segmentState.ElapsedAscent,
                    segmentState.ElapsedDescent);
            }

            var distance = position.DistanceTo(CurrentPosition);

            if (distance < 100)
            {
                return new OnRouteState(
                    RiderId,
                    ActivityId,
                    CurrentPosition, // Use the last known position on the segment
                    CurrentSegment, // Use the current segment of the route
                    Route,
                    Direction,
                    ElapsedDistance,
                    ElapsedAscent,
                    ElapsedDescent);
            }

            return segmentState;
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
                    return new UpcomingTurnState(RiderId, ActivityId, CurrentPosition, CurrentSegment, Route, Direction, x, ElapsedDistance, ElapsedAscent, ElapsedDescent);
                }

                // Three-way junctions are somehow screwed because we only get 2 turn
                // commands and it's never the same ones...
                // To work around that we check if the upcoming turns on this segment
                // have three other segments and then return the UpcomingTurnState with
                // hardcoded turn directions (because all of them apply...)
                if (x.Count == 2 && 
                    CurrentSegment.NextSegments(Direction).Count == 3)
                {
                    // We've got all the turn commands for this segment
                    return new UpcomingTurnState(RiderId, ActivityId, CurrentPosition, CurrentSegment, Route, Direction, 
                        new List<TurnDirection>
                        {
                            TurnDirection.Left,
                            TurnDirection.GoStraight,
                            TurnDirection.Right
                        },
                        ElapsedDistance, ElapsedAscent, ElapsedDescent);
                }

                // Add the new list of turn directions to
                // a new state.
                return new OnRouteState(RiderId, ActivityId, CurrentPosition, CurrentSegment, Route, Direction, x, ElapsedDistance, ElapsedAscent, ElapsedDescent);
            }

            return this;
        }

        private SegmentDirection DetermineSegmentDirection(Segment newSegment, TrackPoint newPosition)
        {
            if (newSegment.Id == CurrentSegment.Id)
            {
                int previousPositionIndex;
                int currentPositionIndex;

                if (CurrentPosition.Index.HasValue && newPosition.Index.HasValue)
                {
                    previousPositionIndex = CurrentPosition.Index.Value;
                    currentPositionIndex = newPosition.Index.Value;
                }
                else
                {
                    previousPositionIndex = newSegment.Points.IndexOf(CurrentPosition);
                    currentPositionIndex = newSegment.Points.IndexOf(newPosition);
                }

                if (previousPositionIndex == -1 || currentPositionIndex == -1)
                {
                    return SegmentDirection.Unknown;
                }

                if (previousPositionIndex < currentPositionIndex)
                {
                    return SegmentDirection.AtoB;
                }

                if (previousPositionIndex > currentPositionIndex)
                {
                    return SegmentDirection.BtoA;
                }
                // If the indexes of the positions are the same then 
                // keep the same direction as before to ensure we
                // don't revert to Unknown unnecessarily.
                return Direction;
            }

            return SegmentDirection.Unknown;
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
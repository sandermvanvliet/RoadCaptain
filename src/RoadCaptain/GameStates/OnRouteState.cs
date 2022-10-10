// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public sealed class OnRouteState : GameState
    {
        public const int RouteLockPositionRadius = 35;
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
        public override uint RiderId { get; }

        [JsonProperty]
        public ulong ActivityId { get; }

        [JsonProperty]
        public TrackPoint CurrentPosition { get; }

        [JsonProperty]
        public Segment CurrentSegment { get; }

        [JsonProperty]
        public SegmentDirection Direction { get; private set; }

        [JsonProperty]
        public double ElapsedDistance { get; }

        [JsonProperty]
        public double ElapsedDescent { get; }

        [JsonProperty]
        public double ElapsedAscent { get; }

        [JsonProperty]
        public PlannedRoute Route { get; private set; }

        [JsonProperty]
        public List<TurnDirection> TurnCommands { get; } = new();

        public override GameState EnterGame(uint riderId, ulong activityId)
        {
            // There are cases where Zwift sends this for an ongoing activity
            // so there we remain in the same state.
            if (RiderId == riderId && ActivityId == activityId)
            {
                return this;
            }

            throw InvalidStateTransitionException.AlreadyInGame(GetType());
        }

        public override GameState LeaveGame()
        {
            return new ConnectedToZwiftState();
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            if (!plannedRoute.HasStarted)
            {
                throw InvalidStateTransitionException.RouteNotStarted(GetType());
            }

            if (plannedRoute.HasCompleted)
            {
                throw InvalidStateTransitionException.RouteCompleted(GetType());
            }

            if (!CurrentPosition.Index.HasValue)
            {
                throw new InvalidOperationException(
                    "Current position doesn't have an index value, did you forget to initialize it properly?");
            }

            // No need to worry about segment direction, just take
            // (at max) 10 positions before and 10 after the current
            // position. If the current position is at the start or
            // end of the current segment we'll end up with the next
            // 10 positions which is enough.
            var startIndex = Math.Max(0, CurrentPosition.Index.Value - 10);
            var nextFewPoints = CurrentSegment.Points.Skip(startIndex).Take(20).ToList();

            // When approaching the end of a segment, include the next
            // 10 positions on the next segment but only if we're not
            // at the end of the route yet. If that is the case we'll
            // drop into the branch where we find a match based on all
            // segments rather than the current one.
            if (IsNearingEndOfSegment() && plannedRoute.NextSegmentId != null)
            {
                var nextSegmentOnRoute = segments.Single(segment => segment.Id == plannedRoute.NextSegmentId);

                // Handle looped routes
                var nextSegmentSequenceOnRoute = plannedRoute.IsOnLastSegment
                    ? plannedRoute.CurrentSegmentSequence
                    : plannedRoute.NextSegmentSequence;

                var directionOnNextSegment = nextSegmentSequenceOnRoute.Direction;

                if (directionOnNextSegment == SegmentDirection.AtoB)
                {
                    nextFewPoints.AddRange(nextSegmentOnRoute.Points.Take(10).ToList());
                }
                else
                {
                    nextFewPoints.AddRange(nextSegmentOnRoute.Points.Skip(nextSegmentOnRoute.Points.Count - 10).Take(10).ToList());
                }
            }

            Segment? segment;

            var closestOnSegment = nextFewPoints
                .Where(trackPoint => trackPoint.IsCloseTo(position))
                .MinBy(trackPoint => trackPoint.DistanceTo(position));

            if (closestOnSegment != null)
            {
                segment = closestOnSegment.Segment;
            }
            else
            {
                // This means that the current position isn't on any of the route
                // segments. Instead of immediately going to LostRouteLock state
                // first check if we can match any position at all.

                // Note: We're using an IEnumerable<T> here to prevent
                //       unnecessary ToList() calls because the foreach
                //       loop in GetClosestMatchingSegment handles that
                //       for us.
                var matchingSegments = segments.Where(s => s.Contains(position));
                (segment, closestOnSegment) = matchingSegments.GetClosestMatchingSegment(position, CurrentPosition);
            }
            
            if (segment == null || closestOnSegment == null)
            {
                // To prevent situations at intersections where we go from one segment
                // to the next on the route but briefly touch an unrelated segment we'll
                // use a window in relation to the last known position. Only after
                // the current position is on an unrelated segment and it's more than
                // 25 meters away, then we consider the route lock as lost.
                if (CurrentPosition.DistanceTo(position) < RouteLockPositionRadius)
                {
                    return this;
                }

                // When we get to the end of a segment it can happen that the segment
                // doesn't follow the exact route of the rider. For example the triangle
                // like junctions where we've only mapped the segment going in one
                // direction but don't have anything for the other leg.
                // This is something that usually only happens in Watopia as the segment
                // splitter is now a lot better at aligning junctions.
                if (IsNearingEndOfSegment())
                {
                    // Basically what happens here is that we try to find a point
                    // on the next segment that is within the 25m radius and assume
                    // we're on our way to that one.
                    var closest = nextFewPoints
                        .Select(point =>
                            new
                            {
                                Point = point,
                                Distance = point.DistanceTo(position)
                            })
                        .OrderBy(x => x.Distance)
                        .First();

                    if (closest.Distance < RouteLockPositionRadius)
                    {
                        segment = closest.Point.Segment;
                        closestOnSegment = closest.Point;
                    }
                    else
                    {
                        return new PositionedState(RiderId, ActivityId, position);
                    }
                }
                else
                {
                    return new PositionedState(RiderId, ActivityId, position);
                }
            }

            var positionDelta = CurrentPosition.DeltaTo(closestOnSegment);

            var distance = ElapsedDistance + positionDelta.Distance;
            var ascent = ElapsedAscent + positionDelta.Ascent;
            var descent = ElapsedDescent + positionDelta.Descent;
            var direction = DetermineSegmentDirection(segment, closestOnSegment);

            if (!plannedRoute.IsOnLastSegment && segment.Id != Route.CurrentSegmentId && segment.Id != Route.NextSegmentId)
            {
                var distanceToLast = CurrentPosition.DistanceTo(position);

                // To prevent situations at intersections where we go from one segment
                // to the next on the route but briefly touch an unrelated segment we'll
                // use a window in relation to the last known position. Only after
                // the current position is on an unrelated segment and it's more than
                // 25 meters away, then we consider the route lock as lost.
                if (distanceToLast < RouteLockPositionRadius)
                {
                    return this;
                }

                // Got a point on a segment but it doesn't belong to the route
                // or it belongs to a route segment but is not the current or
                // next one.
                return new LostRouteLockState(
                    RiderId,
                    ActivityId,
                    closestOnSegment,
                    segment,
                    plannedRoute,
                    direction,
                    distance,
                    ascent,
                    descent);
            }

            if (plannedRoute.IsOnLastSegment)
            {
                return HandleEndOfRoute(segments, plannedRoute, segment, closestOnSegment, direction, distance, ascent, descent, RiderId, ActivityId);
            }

            if (plannedRoute.CurrentSegmentId == segment.Id)
            {
                if (direction != SegmentDirection.Unknown && plannedRoute.CurrentSegmentSequence.Direction != direction)
                {
                    return new LostRouteLockState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute,
                        direction, distance, ascent, descent);
                }

                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction, distance, ascent, descent);
            }

            if (plannedRoute.NextSegmentId == segment.Id)
            {
                plannedRoute.EnteredSegment(segment.Id);

                if (direction == SegmentDirection.Unknown)
                {
                    // We can determine the direction based on the turns
                    // because we're entering this segment from a specific
                    // direction.
                    if (segment.NextSegmentsNodeA.Any(turn => turn.SegmentId == CurrentSegment.Id))
                    {
                        // Entering on Node A of the new segment
                        direction = SegmentDirection.AtoB;
                    }
                    else if (segment.NextSegmentsNodeB.Any(turn => turn.SegmentId == CurrentSegment.Id))
                    {
                        direction = SegmentDirection.BtoA;
                    }
                }

                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction, distance, ascent, descent);
            }

            throw new InvalidStateTransitionException("Impossible to work out which state to transition to");
        }

        private bool IsNearingEndOfSegment()
        {
            if (!CurrentPosition.Index.HasValue)
            {
                throw new InvalidOperationException(
                    "Current position doesn't have an index value, did you forget to initialize it properly?");
            }
            return (CurrentPosition.Index.Value < 10 ||
                    CurrentPosition.Index.Value > CurrentSegment.Points.Count - 10);
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
                var x = new List<TurnDirection> { turnDirection };
                x.AddRange(TurnCommands);

                var nextSegments = CurrentSegment.NextSegments(Direction);

                if (x.Count == nextSegments.Count &&
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
                    nextSegments.Count == 3)
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


        public static GameState HandleEndOfRoute(
            List<Segment> segments, 
            PlannedRoute plannedRoute, 
            Segment segment,
            TrackPoint closestOnSegment, 
            SegmentDirection direction, 
            double distance, 
            double ascent, 
            double descent, 
            uint riderId, 
            ulong activityId)
        {
            if (plannedRoute.CurrentSegmentId != segment.Id)
            {
                var lastOfRoute = plannedRoute.CurrentSegmentSequence;
                var lastSegmentOfRoute = segments.Single(s => s.Id == plannedRoute.CurrentSegmentId);
                if (lastOfRoute.Direction == SegmentDirection.AtoB)
                {
                    if (lastSegmentOfRoute.NextSegmentsNodeB.Any(t =>
                            t.SegmentId == segment.Id))
                    {
                        // Moved from last segment of route to the next segment at the end of that
                        return new CompletedRouteState(riderId, activityId, closestOnSegment, plannedRoute);
                    }

                    // TODO reproduce this with the ItalianVillasRepro route
                    return new LostRouteLockState(riderId, activityId, closestOnSegment, segment, plannedRoute, direction,
                        distance, ascent, descent);
                }

                if (lastOfRoute.Direction == SegmentDirection.BtoA)
                {
                    if (lastSegmentOfRoute.NextSegmentsNodeA.Any(t =>
                            t.SegmentId == segment.Id))
                    {
                        // Moved from last segment of route to the next segment at the end of that
                        return new CompletedRouteState(riderId, activityId, closestOnSegment,
                            plannedRoute);
                    }

                    return new LostRouteLockState(riderId, activityId, closestOnSegment, segment, plannedRoute, direction,
                        distance, ascent, descent);
                }
            }

            return new OnRouteState(riderId, activityId, closestOnSegment, segment, plannedRoute, direction, distance, ascent,
                descent);
        }
    }
}

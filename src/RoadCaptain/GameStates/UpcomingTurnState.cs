// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public sealed class UpcomingTurnState : GameState
    {
        public UpcomingTurnState(uint riderId,
            ulong activityId,
            TrackPoint currentPosition,
            Segment segment,
            PlannedRoute plannedRoute,
            SegmentDirection direction,
            List<TurnDirection> directions,
            double elapsedDistance,
            double elapsedAscent,
            double elapsedDescent)
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
            Directions = directions;
        }


        [JsonProperty] public override uint RiderId { get; }

        [JsonProperty] public ulong ActivityId { get; }

        [JsonProperty] public TrackPoint CurrentPosition { get; }

        [JsonProperty] public Segment CurrentSegment { get; }

        [JsonProperty] public SegmentDirection Direction { get; private set; }

        [JsonProperty] public PlannedRoute Route { get; set; }

        [JsonProperty] public double ElapsedDistance { get; }

        [JsonProperty] public double ElapsedDescent { get; }

        [JsonProperty] public double ElapsedAscent { get; }

        [JsonProperty] public List<TurnDirection> Directions { get; private set; }

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
                if (CurrentPosition.DistanceTo(position) < OnRouteState.RouteLockPositionRadius)
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

                    if (closest.Distance < OnRouteState.RouteLockPositionRadius)
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

            if (plannedRoute.CurrentSegmentId == segment.Id)
            {
                if (direction != Direction)
                {
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

                return new UpcomingTurnState(
                    RiderId, 
                    ActivityId, 
                    closestOnSegment, 
                    segment, 
                    plannedRoute,
                    direction,
                    Directions,
                    distance, 
                    ascent, 
                    descent);
            }

            if (plannedRoute.NextSegmentId == segment.Id)
            {
                plannedRoute.EnteredSegment(segment.Id);
                return new OnRouteState(
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

            if (segment.Id == CurrentSegment.Id)
            {
                if (closestOnSegment.Equals(CurrentPosition))
                {
                    // We're still on the same segment
                    return this;
                }

                return new UpcomingTurnState(
                    RiderId, 
                    ActivityId,
                    closestOnSegment, 
                    segment, 
                    plannedRoute,
                    Direction,
                    Directions,
                    distance, 
                    ascent, 
                    descent);
            }

            if (plannedRoute.IsOnLastSegment)
            {
                return OnRouteState.HandleEndOfRoute(segments, plannedRoute, segment, closestOnSegment, direction, distance, ascent, descent, RiderId, ActivityId);
            }

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
    }
}

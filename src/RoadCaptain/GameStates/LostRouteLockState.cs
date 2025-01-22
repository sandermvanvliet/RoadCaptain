// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public sealed class LostRouteLockState : GameState
    {
        public LostRouteLockState(
            uint riderId,
            ulong activityId,
            TrackPoint currentPosition,
            Segment segment,
            PlannedRoute plannedRoute,
            SegmentDirection direction,
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
            Route = plannedRoute;
        }

        [JsonProperty] public override uint RiderId { get; }
        [JsonProperty] public ulong ActivityId { get; }
        [JsonProperty] public TrackPoint CurrentPosition { get; }
        [JsonProperty] public Segment CurrentSegment { get; }
        [JsonProperty] public SegmentDirection Direction { get; private set; }
        [JsonProperty] public double ElapsedDistance { get; }
        [JsonProperty] public double ElapsedDescent { get; }
        [JsonProperty] public double ElapsedAscent { get; }
        [JsonProperty] public PlannedRoute Route { get; private set; }

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

            var distance = ElapsedDistance + positionDelta.Distance;
            var ascent = ElapsedAscent + positionDelta.Ascent;
            var descent = ElapsedDescent + positionDelta.Descent;
            var direction = DetermineSegmentDirection(segment, closestOnSegment);

            if (plannedRoute.CurrentSegmentId == segment.Id)
            {
                // CurrentSegmentSequence is never null because we check CurrentSegmentId for null above
                if(plannedRoute.CurrentSegmentSequence!.Direction == direction)
                {
                    return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction,
                    distance, ascent, descent);
                }

                return new LostRouteLockState(RiderId, ActivityId, closestOnSegment,
                    segment, Route, direction, distance, ascent, descent);
            }

            if (plannedRoute.NextSegmentId == segment.Id)
            {
                plannedRoute.EnteredSegment(segment.Id);
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction,
                    distance, ascent, descent);
            }

            var segmentState = new OnSegmentState(RiderId, ActivityId, closestOnSegment, segment, direction, distance,
                ascent, descent);

            if (plannedRoute.IsOnLastSegment)
            {
                return OnRouteState.HandleEndOfRoute(segments, plannedRoute, segment, closestOnSegment, direction, distance, ascent, descent, RiderId, ActivityId);
            }

            if (segmentState.CurrentSegment.Id == Route.CurrentSegmentId)
            {
                return new OnRouteState(RiderId, ActivityId, segmentState.CurrentPosition,
                    segmentState.CurrentSegment, Route, segmentState.Direction, segmentState.ElapsedDistance,
                    segmentState.ElapsedAscent, segmentState.ElapsedDescent);
            }

            // From LostRouteLockState we can't go to OnSegmentState because that would mean
            // we've never started a route which we did, otherwise we wouldn't have lost
            // the lock.
            return new LostRouteLockState(RiderId, ActivityId, segmentState.CurrentPosition,
                segmentState.CurrentSegment, Route, segmentState.Direction,
                segmentState.ElapsedDistance, segmentState.ElapsedAscent, segmentState.ElapsedDescent);
        }

        public override GameState TurnCommandAvailable(string type)
        {
            throw InvalidStateTransitionException.NotOnARoute(GetType());
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

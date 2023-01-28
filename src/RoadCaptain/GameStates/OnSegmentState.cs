// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public sealed class OnSegmentState : GameState
    {
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
        
        public OnSegmentState(uint riderId, ulong activityId, TrackPoint currentPosition, Segment segment,
            SegmentDirection direction, double elapsedDistance, double elapsedAscent, double elapsedDescent) 
        {
            RiderId = riderId;
            ActivityId = activityId;
            CurrentPosition = currentPosition;
            CurrentSegment = segment;
            Direction = direction;
            ElapsedDistance = elapsedDistance;
            ElapsedAscent = elapsedAscent;
            ElapsedDescent = elapsedDescent;
        }
        
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

        public override GameState TurnCommandAvailable(string type)
        {
            throw InvalidStateTransitionException.NotOnARoute(GetType());
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            // Note: We're using an IEnumerable<T> here to prevent
            //       unnecessary ToList() calls because the foreach
            //       loop in GetClosestMatchingSegment handles that
            //       for us.
            var matchingSegments = segments.Where(s => s.Contains(position));
            
            var (segment, closestOnSegment) = matchingSegments.GetClosestMatchingSegment( position, CurrentPosition);

            if (segment == null || closestOnSegment == null)
            {
                return new PositionedState(RiderId, ActivityId, position);
            }

            var positionDelta = CurrentPosition.DeltaTo(closestOnSegment);

            var distance = ElapsedDistance + positionDelta.Distance;
            var ascent = ElapsedAscent + positionDelta.Ascent;
            var descent = ElapsedDescent + positionDelta.Descent;
            var direction = DetermineSegmentDirection(segment, closestOnSegment);

            if (!plannedRoute.HasStarted && plannedRoute.StartingSegmentId == segment.Id && direction == plannedRoute.RouteSegmentSequence[0].Direction)
            {
                plannedRoute.EnteredSegment(segment.Id);

                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction, distance, ascent, descent);
            }

            if (!plannedRoute.HasStarted && plannedRoute.StartingSegmentId == segment.Id && direction != SegmentDirection.Unknown && direction != plannedRoute.RouteSegmentSequence[0].Direction)
            {
                return new OnSegmentState(RiderId, ActivityId, closestOnSegment, segment,direction, distance, ascent, descent);
            }

            // If a connection lost event has happened and the user re-enters the game
            // then the route has already started and we need to continue with the route
            if (plannedRoute.HasStarted && 
                plannedRoute.CurrentSegmentId == segment.Id &&
                direction != SegmentDirection.Unknown && 
                plannedRoute.CurrentSegmentSequence!.Direction == direction)
            {
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction, distance, ascent, descent);
            }

            // This is the same case as above but the rider has progressed to the next
            // segment. For example where the end of the current segment was reached
            // during reconnection and the rider still moved forward
            if (plannedRoute.HasStarted && 
                plannedRoute.NextSegmentId == segment.Id &&
                direction != SegmentDirection.Unknown && 
                plannedRoute.NextSegmentSequence!.Direction == direction)
            {
                // Progress the route
                plannedRoute.EnteredSegment(segment.Id);

                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction, distance, ascent, descent);
            }

            if(plannedRoute.HasStarted)
            {
                // If a route has been started you can only go to LostRouteLockState,
                // OnRouteState, CompletedRouteState or UpcomingTurnState. Going back
                // to OnSegmentState means that something has gone wrong in any of
                // those states.
                throw new InvalidStateTransitionException("A started route can never result in a OnSegmentState, only an OnRouteState");
            }
            
            return new OnSegmentState(RiderId, ActivityId, closestOnSegment, segment, direction, distance, ascent, descent);
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

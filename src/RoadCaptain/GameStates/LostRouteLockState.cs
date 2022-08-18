using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class LostRouteLockState : GameState
    {
        public LostRouteLockState(uint riderId, ulong activityId, TrackPoint currentPosition, Segment segment,
            SegmentDirection direction, PlannedRoute plannedRoute, double elapsedDistance, double elapsedAscent,
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

        [JsonProperty] public sealed override uint RiderId { get; }

        [JsonProperty] public ulong ActivityId { get; }

        [JsonProperty] public TrackPoint CurrentPosition { get; }

        [JsonProperty] public Segment CurrentSegment { get; }

        [JsonProperty] public SegmentDirection Direction { get; private set; }

        public double ElapsedDistance { get; }

        public double ElapsedDescent { get; }

        public double ElapsedAscent { get; }

        [JsonProperty] public PlannedRoute Route { get; private set; }

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
            var result = BaseUpdatePosition(position, segments, plannedRoute);

            return result;
        }

        public override GameState TurnCommandAvailable(string type)
        {
            throw InvalidStateTransitionException.NotOnARoute(GetType());
        }

        public GameState BaseUpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
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

            var distance = ElapsedDistance + positionDelta.Distance;
            var ascent = ElapsedAscent + positionDelta.Ascent;
            var descent = ElapsedDescent + positionDelta.Descent;
            var direction = DetermineSegmentDirection(segment, closestOnSegment);

            if (!plannedRoute.HasStarted && plannedRoute.StartingSegmentId == segment.Id)
            {
                plannedRoute.EnteredSegment(segment.Id);
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction, distance, ascent, descent);
            }

            if (plannedRoute.HasStarted && !plannedRoute.HasCompleted && plannedRoute.CurrentSegmentId == segment.Id)
            {
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction, distance, ascent, descent);
            }

            if (plannedRoute.HasStarted && plannedRoute.NextSegmentId == segment.Id)
            {
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction, distance, ascent, descent);
            }

            var segmentState = new OnSegmentState(RiderId, ActivityId, closestOnSegment, segment, direction, distance, ascent, descent);

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
                            segmentState.CurrentSegment, segmentState.Direction, plannedRoute,
                            segmentState.ElapsedDistance, segmentState.ElapsedAscent, segmentState.ElapsedDescent);
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
                            segmentState.CurrentSegment, segmentState.Direction, plannedRoute,
                            segmentState.ElapsedDistance, segmentState.ElapsedAscent, segmentState.ElapsedDescent);
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

                return new OnRouteState(RiderId, ActivityId, segmentState.CurrentPosition,
                    segmentState.CurrentSegment, Route, segmentState.Direction, segmentState.ElapsedDistance,
                    segmentState.ElapsedAscent, segmentState.ElapsedDescent);
            }

            if (segmentState.CurrentSegment.Id == Route.CurrentSegmentId)
            {
                return new OnRouteState(RiderId, ActivityId, segmentState.CurrentPosition,
                    segmentState.CurrentSegment, Route, segmentState.Direction, segmentState.ElapsedDistance,
                    segmentState.ElapsedAscent, segmentState.ElapsedDescent);
            }

            var distanceTo = position.DistanceTo(CurrentPosition);

            if (distanceTo < 100)
            {
                return new OnRouteState(
                    RiderId,
                    ActivityId,
                    CurrentPosition, // Use the last known position on the segment
                    CurrentSegment, // Use the current segment of the route
                    Route,
                    direction,
                    distance,
                    ascent,
                    descent);
            }

            return segmentState;
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
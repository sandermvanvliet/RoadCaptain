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
                if (plannedRoute.CurrentSegmentId != segment.Id)
                {
                    var lastOfRoute = plannedRoute.RouteSegmentSequence[plannedRoute.SegmentSequenceIndex];
                    var lastSegmentOfRoute = segments.Single(s => s.Id == plannedRoute.CurrentSegmentId);
                    if (lastOfRoute.Direction == SegmentDirection.AtoB)
                    {
                        if (lastSegmentOfRoute.NextSegmentsNodeB.Any(t =>
                                t.SegmentId == segment.Id))
                        {
                            // Moved from last segment of route to the next segment at the end of that
                            return new CompletedRouteState(RiderId, ActivityId, closestOnSegment, plannedRoute);
                        }

                        // TODO reproduce this with the ItalianVillasRepro route
                        return new LostRouteLockState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction, distance, ascent, descent);
                    }

                    if (lastOfRoute.Direction == SegmentDirection.BtoA)
                    {
                        if (lastSegmentOfRoute.NextSegmentsNodeA.Any(t =>
                                t.SegmentId == segment.Id))
                        {
                            // Moved from last segment of route to the next segment at the end of that
                            return new CompletedRouteState(RiderId, ActivityId, closestOnSegment,
                                plannedRoute);
                        }

                        return new LostRouteLockState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction, distance, ascent, descent);
                    }
                }

                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, direction, distance, ascent, descent);
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
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class LostRouteLockState : GameState
    {
        public LostRouteLockState(uint riderId, ulong activityId, TrackPoint currentPosition, Segment segment,
            SegmentDirection direction, PlannedRoute plannedRoute, double elapsedDistance, double elapsedAscent, double elapsedDescent) 
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
            var result = BaseBaseUpdatePosition(position, segments, plannedRoute);

            if (result is OnSegmentState segmentState)
            {
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
                            return new LostRouteLockState(RiderId, ActivityId ,segmentState.CurrentPosition, segmentState.CurrentSegment, segmentState.Direction, plannedRoute, segmentState.ElapsedDistance, segmentState.ElapsedAscent, segmentState.ElapsedDescent);
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
                            
                            return new LostRouteLockState(RiderId, ActivityId ,segmentState.CurrentPosition, segmentState.CurrentSegment, segmentState.Direction, plannedRoute, segmentState.ElapsedDistance, segmentState.ElapsedAscent, segmentState.ElapsedDescent);
                        }
                    }

                    return new OnRouteState(RiderId, ActivityId, segmentState.CurrentPosition,
                        segmentState.CurrentSegment, Route, segmentState.Direction, segmentState.ElapsedDistance, segmentState.ElapsedAscent, segmentState.ElapsedDescent);
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

                    return new OnRouteState(RiderId, ActivityId, segmentState.CurrentPosition, segmentState.CurrentSegment, Route, segmentState.Direction, segmentState.ElapsedDistance, segmentState.ElapsedAscent, segmentState.ElapsedDescent);
                }

                if (segmentState.CurrentSegment.Id == Route.CurrentSegmentId)
                {
                    return new OnRouteState(RiderId, ActivityId, segmentState.CurrentPosition, segmentState.CurrentSegment, Route, segmentState.Direction, segmentState.ElapsedDistance, segmentState.ElapsedAscent, segmentState.ElapsedDescent);
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
                        Direction,
                        ElapsedDistance,
                        ElapsedAscent,
                        ElapsedDescent);
                }

                return segmentState;
            }

            return result;
        }

        private GameState BaseBaseUpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
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

            if (!plannedRoute.HasStarted && plannedRoute.StartingSegmentId == segment.Id)
            {
                plannedRoute.EnteredSegment(segment.Id);
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, Direction, ElapsedDistance, ElapsedAscent, ElapsedDescent);
            }

            if (plannedRoute.HasStarted && !plannedRoute.HasCompleted && plannedRoute.CurrentSegmentId == segment.Id)
            {
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, Direction, ElapsedDistance, ElapsedAscent, ElapsedDescent);
            }
            
            if (plannedRoute.HasStarted && plannedRoute.NextSegmentId == segment.Id)
            {
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, Direction, ElapsedDistance, ElapsedAscent, ElapsedDescent);
            }

            return new OnSegmentState(RiderId, ActivityId, closestOnSegment, segment, Direction, ElapsedDistance, ElapsedAscent, ElapsedDescent);
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

        public double ElapsedDistance { get; private set; }

        public double ElapsedDescent { get; private set; }

        public double ElapsedAscent { get; private set; }

        [JsonProperty]
        public PlannedRoute Route { get; private set; }
    }
}
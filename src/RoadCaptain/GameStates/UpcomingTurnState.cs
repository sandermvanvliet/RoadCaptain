using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class UpcomingTurnState : GameState
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
        public PlannedRoute Route { get; set; }

        public double ElapsedDistance { get; }

        public double ElapsedDescent { get; }

        public double ElapsedAscent { get; }

        [JsonProperty]
        public List<TurnDirection> Directions { get; private set; }
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

            if (result is OnRouteState routeState)
            {
                if (routeState.CurrentSegment.Id == CurrentSegment.Id)
                {
                    if (routeState.CurrentPosition.Equals(CurrentPosition))
                    {
                        // We're still on the same segment
                        return this;
                    }

                    return new UpcomingTurnState(RiderId, ActivityId, routeState.CurrentPosition, routeState.CurrentSegment, plannedRoute, routeState.Direction, Directions, routeState.ElapsedDistance, routeState.ElapsedAscent, routeState.ElapsedDescent);
                }
            }

            return result;
        }

        private GameState BaseUpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
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

        public override GameState TurnCommandAvailable(string type)
        {
            return this;
        }
    }
}
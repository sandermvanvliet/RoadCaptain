using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class OnSegmentState : GameState
    {
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
            throw new InvalidStateTransitionException("User is already in-game");
        }

        public override GameState LeaveGame()
        {
            return new ConnectedToZwiftState();
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            var result = BaseUpdatePosition(position, segments, plannedRoute);
            
            if (result is OnSegmentState segmentState)
            {
                var positionDelta = CurrentPosition.DeltaTo(segmentState.CurrentPosition);

                segmentState.ElapsedDistance += positionDelta.Distance;
                segmentState.ElapsedAscent += positionDelta.Ascent;
                segmentState.ElapsedDescent += positionDelta.Descent;

                UpdateDirection(segmentState);
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
            
            var (segment, closestOnSegment) = matchingSegments.GetClosestMatchingSegment( position, CurrentPosition);

            if (segment == null || closestOnSegment == null)
            {
                return new PositionedState(RiderId, ActivityId, position);
            }

            // This is to ensure that we have the segment of the position
            // for future reference.
            closestOnSegment.Segment = segment;

            var positionDelta = CurrentPosition.DeltaTo(closestOnSegment);

            var distance = ElapsedDistance + positionDelta.Distance;
            var ascent = ElapsedAscent + positionDelta.Ascent;
            var descent = ElapsedDescent + positionDelta.Descent;

            if (!plannedRoute.HasStarted && plannedRoute.StartingSegmentId == segment.Id)
            {
                plannedRoute.EnteredSegment(segment.Id);

                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, Direction, distance, ascent, descent);
            }

            if (plannedRoute.HasStarted && !plannedRoute.HasCompleted && plannedRoute.CurrentSegmentId == segment.Id)
            {
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, Direction, distance, ascent, descent);
            }
            
            if (plannedRoute.HasStarted && plannedRoute.NextSegmentId == segment.Id)
            {
                return new OnRouteState(RiderId, ActivityId, closestOnSegment, segment, plannedRoute, Direction, distance, ascent, descent);
            }

            return new OnSegmentState(RiderId, ActivityId, closestOnSegment, segment, Direction, distance, ascent, descent);
        }

        public override GameState TurnCommandAvailable(string type)
        {
            return this;
        }

        private void UpdateDirection(OnSegmentState segmentState)
        {
            if (segmentState.CurrentSegment.Id == CurrentSegment.Id)
            {
                int previousPositionIndex;
                int currentPositionIndex;

                if (CurrentPosition.Index.HasValue && segmentState.CurrentPosition.Index.HasValue)
                {
                    previousPositionIndex = CurrentPosition.Index.Value;
                    currentPositionIndex = segmentState.CurrentPosition.Index.Value;
                }
                else
                {
                    previousPositionIndex = segmentState.CurrentSegment.Points.IndexOf(CurrentPosition);
                    currentPositionIndex = segmentState.CurrentSegment.Points.IndexOf(segmentState.CurrentPosition);
                }

                if (previousPositionIndex == -1 || currentPositionIndex == -1)
                {
                    segmentState.Direction = SegmentDirection.Unknown;
                }
                else
                {
                    if (previousPositionIndex < currentPositionIndex)
                    {
                        segmentState.Direction = SegmentDirection.AtoB;
                    }
                    else if (previousPositionIndex > currentPositionIndex)
                    {
                        segmentState.Direction = SegmentDirection.BtoA;
                    }
                    else
                    {
                        // If the indexes of the positions are the same then 
                        // keep the same direction as before to ensure we
                        // don't revert to Unknown unnecessarily.
                        segmentState.Direction = Direction;
                    }
                }
            }
            else
            {
                segmentState.Direction = SegmentDirection.Unknown;
            }
        }
    }
}
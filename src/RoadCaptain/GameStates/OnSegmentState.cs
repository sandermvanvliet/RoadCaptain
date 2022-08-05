using System.Collections.Generic;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class OnSegmentState : PositionedState
    {
        [JsonProperty]
        public Segment CurrentSegment { get; private set; }

        [JsonProperty]
        public SegmentDirection Direction { get; private set; } = SegmentDirection.Unknown;

        public double ElapsedDistance { get; private set; }

        public double ElapsedDescent { get; private set; }

        public double ElapsedAscent { get; private set; }

        public OnSegmentState(uint riderId, ulong activityId, TrackPoint currentPosition, Segment segment) 
            : base(riderId, activityId, currentPosition)
        {
            CurrentSegment = segment;
        }

        protected OnSegmentState(uint riderId, ulong activityId, TrackPoint currentPosition, Segment segment,
            SegmentDirection direction, double elapsedDistance, double elapsedAscent, double elapsedDescent) 
            : this(riderId, activityId, currentPosition, segment)
        {
            Direction = direction;
            ElapsedDistance = elapsedDistance;
            ElapsedAscent = elapsedAscent;
            ElapsedDescent = elapsedDescent;
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            var result = base.UpdatePosition(position, segments, plannedRoute);

            if (result is OnSegmentState segmentState)
            {
                var distanceFromLast = CurrentPosition.DistanceTo(segmentState.CurrentPosition);

                segmentState.ElapsedDistance = ElapsedDistance + (distanceFromLast / 1000);

                if (CurrentPosition.Altitude < segmentState.CurrentPosition.Altitude)
                {
                    segmentState.ElapsedAscent = ElapsedAscent + (segmentState.CurrentPosition.Altitude - CurrentPosition.Altitude);
                }
                else if (CurrentPosition.Altitude > segmentState.CurrentPosition.Altitude)
                {
                    segmentState.ElapsedDescent = ElapsedDescent + (CurrentPosition.Altitude - segmentState.CurrentPosition.Altitude);
                }

                UpdateDirection(segmentState);
            }

            return result;
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
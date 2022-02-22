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

        public OnSegmentState(ulong activityId, TrackPoint currentPosition, Segment segment) 
            : base(activityId, currentPosition)
        {
            CurrentSegment = segment;
        }

        protected OnSegmentState(ulong activityId, TrackPoint currentPosition, Segment segment, SegmentDirection direction) 
            : this(activityId, currentPosition, segment)
        {
            Direction = direction;
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            var result = base.UpdatePosition(position, segments, plannedRoute);

            if (result is OnSegmentState segmentState)
            {
                UpdateDirection(position, segmentState);
            }

            return result;
        }

        private void UpdateDirection(TrackPoint position, OnSegmentState segmentState)
        {
            if (segmentState.CurrentSegment.Id == CurrentSegment.Id)
            {
                int previousPositionIndex;
                int currentPositionIndex;

                if (CurrentPosition.Index.HasValue && position.Index.HasValue)
                {
                    previousPositionIndex = CurrentPosition.Index.Value;
                    currentPositionIndex = position.Index.Value;
                }
                else
                {
                    previousPositionIndex = segmentState.CurrentSegment.Points.IndexOf(CurrentPosition);
                    currentPositionIndex = segmentState.CurrentSegment.Points.IndexOf(position);
                }

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
            else
            {
                segmentState.Direction = SegmentDirection.Unknown;
            }
        }
    }
}
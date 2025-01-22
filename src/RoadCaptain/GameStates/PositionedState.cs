// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public sealed class PositionedState : GameState
    {
        [JsonProperty]
        public sealed override uint RiderId { get; }

        [JsonProperty]
        public ulong ActivityId { get; }
        
        [JsonProperty]
        public TrackPoint CurrentPosition { get; }

        public PositionedState(uint riderId, ulong activityId, TrackPoint currentPosition)
        {
            RiderId = riderId;
            ActivityId = activityId;
            CurrentPosition = currentPosition;
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

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
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

            return new OnSegmentState(RiderId, ActivityId, closestOnSegment, segment, SegmentDirection.Unknown, 0, 0, 0);
        }

        public override GameState TurnCommandAvailable(string type)
        {
            throw InvalidStateTransitionException.NotOnARoute(GetType());
        }
    }
}

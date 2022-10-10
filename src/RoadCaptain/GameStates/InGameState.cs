// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public sealed class InGameState : GameState
    {
        [JsonProperty]
        public ulong ActivityId { get; private set; }

        [JsonProperty]
        public override uint RiderId { get; }

        public InGameState(uint riderId, ulong activityId)
        {
            RiderId = riderId;
            ActivityId = activityId;
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            // Note: We're using an IEnumerable<T> here to prevent
            //       unnecessary ToList() calls because the foreach
            //       loop in GetClosestMatchingSegment handles that
            //       for us.
            var matchingSegments = segments.Where(s => s.Contains(position));
            
            var (segment, closestOnSegment) = matchingSegments.GetClosestMatchingSegment(position, TrackPoint.Unknown);

            if (segment == null || closestOnSegment == null)
            {
                return new PositionedState(RiderId, ActivityId, position);
            }
            
            return new OnSegmentState(RiderId, ActivityId, closestOnSegment, segment, SegmentDirection.Unknown, 0, 0, 0);
        }

        public sealed override GameState EnterGame(uint riderId, ulong activityId)
        {
            if (RiderId == riderId && ActivityId == activityId)
            {
                return this;
            }

            return new InGameState(riderId, activityId);
        }

        public sealed override GameState LeaveGame()
        {
            return new ConnectedToZwiftState();
        }

        public override GameState TurnCommandAvailable(string type)
        {
            throw InvalidStateTransitionException.NotOnARoute(GetType());
        }
    }
}

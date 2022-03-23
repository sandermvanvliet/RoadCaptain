// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;

namespace RoadCaptain
{
    public class PlannedRoute
    {
        public string ZwiftRouteName { get; set; }
        public bool HasCompleted { get; private set; }
        public bool HasStarted { get; private set; }

        public int SegmentSequenceIndex { get; private set; }

        public string StartingSegmentId => RouteSegmentSequence[SegmentSequenceIndex].SegmentId;
        public string NextSegmentId => HasStarted ? RouteSegmentSequence[SegmentSequenceIndex].NextSegmentId : null;
        public TurnDirection TurnToNextSegment => HasStarted ? RouteSegmentSequence[SegmentSequenceIndex].TurnToNextSegment : TurnDirection.None;
        public string CurrentSegmentId => HasStarted ? RouteSegmentSequence[SegmentSequenceIndex].SegmentId : null;

        public List<SegmentSequence> RouteSegmentSequence { get; } = new();

        public RouteMoveResult EnteredSegment(string segmentId)
        {
            if (HasCompleted)
            {
                throw new ArgumentException("Route has already completed, can't enter new segment");
            }

            if (CurrentSegmentId == null && segmentId == StartingSegmentId)
            {
                HasStarted = true;

                return RouteMoveResult.StartedRoute;
            }

            if (CurrentSegmentId != null && NextSegmentId == segmentId)
            {
                SegmentSequenceIndex++;

                // Use the segment index instead of comparing the segment
                // id with the id of the last segment because we may pass
                // the same segment multiple times in the course of this
                // route.
                if (SegmentSequenceIndex == RouteSegmentSequence.Count - 1)
                {
                    HasCompleted = true;

                    return RouteMoveResult.CompletedRoute;
                }

                return RouteMoveResult.EnteredNextSegment;
            }

            throw new ArgumentException(
                $"Was expecting {NextSegmentId} but got {segmentId} and that's not a valid route progression");
        }

        public void Reset()
        {
            HasStarted = false;
            HasCompleted = false;
            SegmentSequenceIndex = 0;
        }
    }

    public enum RouteMoveResult
    {
        Unknown,
        StartedRoute,
        EnteredNextSegment,
        CompletedRoute
    }
}

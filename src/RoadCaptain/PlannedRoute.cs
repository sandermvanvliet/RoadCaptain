// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoadCaptain
{
    public class PlannedRoute
    {
        private World? _world;
        private string? _worldId;
        private int _segmentSequenceIndex;
        public string? Name { get; set; }
        public string? ZwiftRouteName { get; set; }
        [JsonIgnore]
        public bool HasCompleted { get; private set; }
        [JsonIgnore]
        public bool HasStarted { get; private set; }
        [JsonIgnore]
        public bool IsOnLastSegment => SegmentSequenceIndex == RouteSegmentSequence.Count - 1;

        [JsonIgnore]
        public int SegmentSequenceIndex
        {
            get => _segmentSequenceIndex;
            private set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(SegmentSequenceIndex),
                        "Segment sequence index can't be less than zero");
                }

                if (value > RouteSegmentSequence.Count - 1)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(SegmentSequenceIndex),
                        "Segment sequence index can't be greater than the number of segments of the route");
                }

                _segmentSequenceIndex = value;
            }
        }

        [JsonIgnore] public string? StartingSegmentId => RouteSegmentSequence[0].SegmentId;
        [JsonIgnore] public string? CurrentSegmentId => CurrentSegmentSequence?.SegmentId;
        [JsonIgnore] public string? NextSegmentId => NextSegmentSequence?.SegmentId;
        [JsonIgnore] public TurnDirection TurnToNextSegment => CurrentSegmentSequence?.TurnToNextSegment ?? TurnDirection.None;
        [JsonIgnore] public SegmentSequence? CurrentSegmentSequence => HasStarted ? RouteSegmentSequence[SegmentSequenceIndex] : null;
        [JsonIgnore]
        public SegmentSequence? NextSegmentSequence
        {
            get
            {
                if (!HasStarted || HasCompleted)
                {
                    return null;
                }

                if (CurrentSegmentSequence!.Type == SegmentSequenceType.LoopEnd)
                {
                    // The next segment is the start of the loop.
                    // It _can_ be that it's the very first segment sequence
                    // however if you have a looped route with a lead-in 
                    // that won't be the case.
                    return RouteSegmentSequence.First(seq => seq.Type == SegmentSequenceType.LoopStart);
                }

                if (SegmentSequenceIndex < RouteSegmentSequence.Count - 1)
                {
                    return RouteSegmentSequence[SegmentSequenceIndex + 1];
                }

                return null;
            }
        }

        public bool IsLoop =>
            RouteSegmentSequence.Count(seq => seq.Type == SegmentSequenceType.Regular) == 0 &&
            RouteSegmentSequence.Count >= 2;
        [JsonIgnore] public int LoopCount { get; private set; } = 1;
        [JsonIgnore] public bool OnLeadIn => HasStarted && CurrentSegmentSequence!.Type == SegmentSequenceType.LeadIn;

        public List<SegmentSequence> RouteSegmentSequence { get; init; } = new();

        [JsonIgnore]
        public World? World
        {
            get => _world;
            set
            {
                _world = value;
                _worldId = value?.Id;
            }
        }

        [JsonProperty("world")]
        public string? WorldId
        {
            get => _world?.Id ?? _worldId;
            set => _worldId = value;
        }

        public SportType Sport { get; set; } = SportType.Unknown;

        public RouteMoveResult EnteredSegment(string segmentId)
        {
            if (!HasStarted && RouteSegmentSequence.Last().Index == 0)
            {
                // Set the index of each segment sequence entry so
                // that we don't have to rely on SegmentSequenceIndex
                // to keep track of that in the views later.
                for (var index = 0; index < RouteSegmentSequence.Count; index++)
                {
                    RouteSegmentSequence[index].Index = index;
                }
            }

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
                if (IsLoop && CurrentSegmentSequence!.Type == SegmentSequenceType.LoopEnd)
                {
                    SegmentSequenceIndex = NextSegmentSequence!.Index;
                    LoopCount++;
                }
                else
                {
                    SegmentSequenceIndex++;
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

        public override string ToString()
        {
            return Name ?? "(unknown)";
        }

        public void Complete()
        {
            HasCompleted = true;
        }

        public List<TrackPoint> GetTrackPoints(List<Segment> segments)
        {
            var trackPointsForRoute = new List<TrackPoint>();
            var routeTrackPointIndex = 0;

            foreach (var seq in RouteSegmentSequence)
            {
                var segment = segments.Single(s => s.Id == seq.SegmentId);
                
                if (seq.Direction == SegmentDirection.AtoB)
                {
                    for (var index = 0; index < segment.Points.Count; index++)
                    {
                        var segmentPoint = segment.Points[index].Clone();
                        segmentPoint.Index = routeTrackPointIndex++;
                        trackPointsForRoute.Add(segmentPoint);
                    }
                }
                else
                {
                    for (var index = segment.Points.Count - 1; index >= 0; index--)
                    {
                        var segmentPoint = segment.Points[index].Clone();
                        segmentPoint.Index = routeTrackPointIndex++;
                        trackPointsForRoute.Add(segmentPoint);
                    }
                }
            }
            
            return trackPointsForRoute;
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

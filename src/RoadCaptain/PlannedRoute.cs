// Copyright (c) 2022 Sander van Vliet
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
        private World _world;
        private string _worldId;
        public string Name { get; set; }
        public string ZwiftRouteName { get; set; }
        [JsonIgnore]
        public bool HasCompleted { get; private set; }
        [JsonIgnore]
        public bool HasStarted { get; private set; }
        [JsonIgnore]
        public bool IsOnLastSegment => SegmentSequenceIndex == RouteSegmentSequence.Count - 1;
        [JsonIgnore]
        public int SegmentSequenceIndex { get; private set; }
        [JsonIgnore]
        public string StartingSegmentId => RouteSegmentSequence[SegmentSequenceIndex].SegmentId;
        [JsonIgnore]
        public string NextSegmentId => HasStarted ? RouteSegmentSequence[SegmentSequenceIndex].NextSegmentId : null;
        [JsonIgnore]
        public TurnDirection TurnToNextSegment => HasStarted ? RouteSegmentSequence[SegmentSequenceIndex].TurnToNextSegment : TurnDirection.None;
        [JsonIgnore]
        public string CurrentSegmentId => HasStarted ? RouteSegmentSequence[SegmentSequenceIndex].SegmentId : null;

        public bool IsLoop =>
            RouteSegmentSequence.Count(seq => seq.Type == SegmentSequenceType.Regular) == 0 &&
            RouteSegmentSequence.Count >= 2;

        public List<SegmentSequence> RouteSegmentSequence { get; } = new();

        [JsonIgnore]
        public World World
        {
            get => _world;
            set
            {
                _world = value;
                _worldId = value.Id;
            }
        }

        [JsonProperty("world")]
        public string WorldId
        {
            get => _world?.Id ?? _worldId;
            set => _worldId = value;
        }

        public SportType Sport { get; set; } = SportType.Unknown;

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
            return Name;
        }

        public void Complete()
        {
            HasCompleted = true;
        }

        public void EnterNextLoop()
        {
            for (var index = 0; index < RouteSegmentSequence.Count; index++)
            {
                if (RouteSegmentSequence[index].Type == SegmentSequenceType.LoopStart)
                {
                    HasStarted = true;
                    HasCompleted = false;
                    SegmentSequenceIndex = index;
                    return;
                }
            }
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

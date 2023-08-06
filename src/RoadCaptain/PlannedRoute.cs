// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public bool IsOnLoop => CurrentSegmentSequence is { Type: SegmentSequenceType.LoopStart or SegmentSequenceType.Loop or SegmentSequenceType.LoopEnd };

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

        [JsonIgnore] public string StartingSegmentId => RouteSegmentSequence[0].SegmentId;
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

                    if (LoopMode == LoopMode.Infinite)
                    {
                        return RouteSegmentSequence.First(seq => seq.Type == SegmentSequenceType.LoopStart);
                    }

                    // If we have done the amount of loops we want, break out
                    if (LoopCount >= NumberOfLoops)
                    {
                        if (SegmentSequenceIndex < RouteSegmentSequence.Count - 1)
                        {
                            return RouteSegmentSequence[SegmentSequenceIndex + 1];
                        }

                        // We've done the amount of loops that was planned and
                        // there is no lead-out after this.
                        return null;
                    }

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
            RouteSegmentSequence.All(seq => seq.Type != SegmentSequenceType.Regular) &&
            RouteSegmentSequence.Count >= 2;
        [JsonIgnore] public int LoopCount { get; private set; } = 1;
        [JsonIgnore] public bool OnLeadIn => HasStarted && CurrentSegmentSequence!.Type == SegmentSequenceType.LeadIn;
        [JsonIgnore] public bool OnLeadOut => HasStarted && CurrentSegmentSequence!.Type == SegmentSequenceType.LeadOut;
        public int? NumberOfLoops { get; set; }


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
        
        public double Distance { get; private set; }
        public double Descent { get; private set; }
        public double Ascent { get; private set; }
        public ImmutableList<TrackPoint> TrackPoints { get; private set; } = ImmutableList<TrackPoint>.Empty;
        public LoopMode LoopMode { get; set; } = LoopMode.Unknown;

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
                    return RouteMoveResult.StartedNewLoop;
                }

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
            return Name ?? "(unknown)";
        }

        public void Complete()
        {
            HasCompleted = true;
        }

        public void CalculateMetrics(List<Segment> segments)
        {
            var trackPointsForRoute = new List<TrackPoint>();
            var routeTrackPointIndex = 0;
            TrackPoint? previous = null;
            var totalDistance = 0d;
            var totalAscent = 0d;
            var totalDescent = 0d;

            foreach (var seq in RouteSegmentSequence)
            {
                var segment = segments.SingleOrDefault(s => s.Id == seq.SegmentId);
                if (segment == null)
                {
                    throw new MissingSegmentException(seq.SegmentId);
                }

                var segmentPoints = segment.Points.AsEnumerable();

                if (seq.Direction == SegmentDirection.BtoA)
                {
                    segmentPoints = segmentPoints.Reverse();
                }

                foreach (var trackPoint in segmentPoints)
                {
                    var segmentPoint = trackPoint.Clone();

                    var distanceFromLast = previous == null
                        ? 0
                        : TrackPoint.GetDistanceFromLatLonInMeters(previous.Latitude, previous.Longitude,
                            segmentPoint.Latitude, segmentPoint.Longitude);

                    totalDistance += distanceFromLast;

                    segmentPoint.Index = routeTrackPointIndex++;
                    segmentPoint.DistanceOnSegment = totalDistance;
                    segmentPoint.DistanceFromLast = distanceFromLast;
                    trackPointsForRoute.Add(segmentPoint);

                    var altitudeDelta = previous == null ? 0 : segmentPoint.Altitude - previous.Altitude;

                    if (altitudeDelta > 0)
                    {
                        totalAscent += altitudeDelta;
                    }
                    else if (altitudeDelta < 0)
                    {
                        totalDescent += Math.Abs(altitudeDelta);
                    }

                    previous = segmentPoint;
                }
            }

            Ascent = totalAscent;
            Descent = totalDescent;
            Distance = totalDistance;
            TrackPoints = trackPointsForRoute.ToImmutableList();
        }

        public static List<(Segment Segment, TrackPoint Start, TrackPoint Finish)> CalculateClimbMarkers(List<Segment> markers, ImmutableArray<TrackPoint> routePoints)
        {
            var result = new List<(Segment Segment, TrackPoint Start, TrackPoint Finish)>();
            Segment? currentClimb = null;
            TrackPoint? start = null;

            var index = 0;
            while (index < routePoints.Length)
            {
                var point = routePoints[index];

                if (currentClimb == null)
                {
                    var climb = markers.SingleOrDefault(m => m.A.IsCloseTo(point));

                    if (climb != null)
                    {
                        currentClimb = climb;
                        start = point;
                    }
                    else
                    {
                        index++;
                        continue;
                    }
                }

                while (index < routePoints.Length)
                {
                    var nextPoint = routePoints[index];
                    // Check if this point is still on the climb
                    if (currentClimb.Contains(nextPoint))
                    {
                        index++;
                        continue;
                    }
                   
                    // Check if the last point was close to the end of the segment
                    if (currentClimb.B.IsCloseTo(routePoints[index - 1]))
                    {
                        // Yup, add this climb
                        result.Add((
                            currentClimb,
                            start!,
                            routePoints[index - 1]
                        ));
                    }

                    currentClimb = null;
                    start = null;

                    break;
                }
            }

            return result;
        }
    }
}

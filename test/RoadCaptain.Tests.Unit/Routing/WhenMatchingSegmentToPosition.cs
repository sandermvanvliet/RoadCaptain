using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace RoadCaptain.Tests.Unit.Routing
{
    public class WhenMatchingSegmentToPosition
    {
        private readonly Segment _segment;

        public WhenMatchingSegmentToPosition()
        {
            _segment = new Segment(
                "S1",
                new List<Position>
                {
                    new(0, 0, 0),
                    new(0, 0.1f, 0),
                    new(0, 0.2f, 0),
                    new(0, 0.3f, 0),
                    new(0, 0.4f, 0),
                    new(0, 0.5f, 0),
                    new(0, 0.6f, 0),
                },
                new List<Turn>
                {
                    new(TurnDirection.StraightOn, "S2"),
                    new(TurnDirection.Left, "S3"),
                },
                new List<Turn>
                {
                    new(TurnDirection.StraightOn, "S4"),
                }
            );
        }

        [Fact]
        public void GivenPositionOutsideSegment_NoMatchIsReturned()
        {
            var position = new Position(2, 2, 0);

            _segment
                .Contains(position)
                .Should()
                .BeFalse();
        }

        [Fact]
        public void GivenPositionOnSegment_MatchIsReturned()
        {
            var position = new Position(0, 0.5f, 0);

            _segment
                .Contains(position)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void GivenPositionOnSegmentButNotExactMatch_MatchIsReturned()
        {
            var position = new Position(0.01f, 0.501f, 0);

            _segment
                .Contains(position)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void GivenPointsInSequenceAtoB_DirectionShouldBeAtoB()
        {
            var first = new Position(0, 0.5f, 0);
            var second = new Position(0, 0.6f, 0);

            _segment
                .DirectionOf(first, second)
                .Should()
                .Be(SegmentDirection.AtoB);
        }

        [Fact]
        public void GivenPointsInSequenceBtoA_DirectionShouldBeBtoA()
        {
            var first = new Position(0, 0.6f, 0);
            var second = new Position(0, 0.5f, 0);

            _segment
                .DirectionOf(first, second)
                .Should()
                .Be(SegmentDirection.BtoA);
        }

        [Fact]
        public void GivenSegmentWithSingleSegmentAfterB_StraightTurnWithNextSegmentIsReturned()
        {
            _segment
                .NextSegments(SegmentDirection.AtoB)
                .Select(kv => kv.Direction)
                .Should()
                .BeEquivalentTo(new[] { TurnDirection.StraightOn });
        }

        [Fact]
        public void GivenSegmentWithTwoSegmentAfterA_StraightOnAndLeftAreReturned()
        {
            _segment
                .NextSegments(SegmentDirection.BtoA)
                .Select(kv => kv.Direction)
                .Should()
                .BeEquivalentTo(new[] { TurnDirection.Left, TurnDirection.StraightOn });
        }
    }

    public class Segment
    {
        public string Id { get; }
        public Position A => _points.First();
        public Position B => _points.Last();
        private readonly List<Position> _points;
        private readonly List<Turn> _nextSegmentsNodeA;
        private readonly List<Turn> _nextSegmentsNodeB;

        public Segment(string id,
            List<Position> points,
            List<Turn> nextSegmentsNodeA,
            List<Turn> nextSegmentsNodeB)
        {
            Id = id;
            _points = points;
            _nextSegmentsNodeA = nextSegmentsNodeA;
            _nextSegmentsNodeB = nextSegmentsNodeB;
        }

        public bool Contains(Position position)
        {
            return _points.Contains(position);
        }

        public SegmentDirection DirectionOf(Position first, Position second)
        {
            var firstIndex = _points.IndexOf(first);
            var secondIndex = _points.IndexOf(second);

            if (firstIndex == -1 || secondIndex == -1)
            {
                return SegmentDirection.Unknown;
            }

            return firstIndex < secondIndex
                ? SegmentDirection.AtoB
                : SegmentDirection.BtoA;
        }

        public List<Turn> NextSegments(SegmentDirection segmentDirection)
        {
            if (segmentDirection == SegmentDirection.AtoB)
            {
                return _nextSegmentsNodeB;
            }

            if (segmentDirection == SegmentDirection.BtoA)
            {
                return _nextSegmentsNodeA;
            }

            throw new ArgumentException(
                "Can't determine next segments for an unknown segmentDirection",
                nameof(segmentDirection));
        }
    }

    public class Turn : IEquatable<Turn>
    {
        public Turn(TurnDirection direction, string segmentId)
        {
            Direction = direction;
            SegmentId = segmentId;
        }

        public TurnDirection Direction { get; }
        public string SegmentId { get; }

        public bool Equals(Turn other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Direction == other.Direction && SegmentId == other.SegmentId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Turn)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Direction, SegmentId);
        }

        public override string ToString()
        {
            return $"{Direction} to {SegmentId}";
        }
    }

    public enum TurnDirection
    {
        StraightOn,
        Left,
        Right,
    }

    public enum SegmentDirection
    {
        Unknown,
        AtoB,
        BtoA
    }

    public class Position : IEquatable<Position>
    {
        public Position(float latitude, float longitude, float altitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
        }

        public float Latitude { get; }
        public float Longitude { get; }
        public float Altitude { get; }
        private const float MatchRadius = 0.025f;

        public bool Equals(Position other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            // TODO: Determine how accurate Zwift is with rider position
            // Currently this allows for a .025 radius around the point
            // to have some fuzzy matching. Most likely the points on our
            // segment will be further apart and we need to increase this.
            // That will be a tricky thing when we arrive at junctions....
            //
            // Note: That can be made to work as long as we determine
            // position with the previous segment in mind. It won't jump
            // from one to the next most likely then.

            var latOffset = Latitude - other.Latitude;
            var latMatch = latOffset >= -MatchRadius && latOffset <= MatchRadius;

            var lonOffset = Longitude - other.Longitude;
            var lonMatch = lonOffset >= -MatchRadius && lonOffset <= MatchRadius;

            var altOffset = Altitude - other.Altitude;
            var altMatch = altOffset >= -MatchRadius && altOffset <= MatchRadius;

            return latMatch && lonMatch && altMatch;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Position)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Latitude, Longitude, Altitude);
        }
    }
}
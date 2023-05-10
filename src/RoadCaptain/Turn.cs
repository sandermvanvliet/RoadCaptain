// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain
{
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

        public override bool Equals(object? obj)
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
}

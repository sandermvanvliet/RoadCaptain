// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;

namespace RoadCaptain
{
    public class SpawnPointComparer : IEqualityComparer<SpawnPoint>
    {
        public bool Equals(SpawnPoint? x, SpawnPoint? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.SegmentId == y.SegmentId && x.Direction == y.Direction;
        }

        public int GetHashCode(SpawnPoint obj)
        {
            return HashCode.Combine(obj.SegmentId, (int)obj.Direction);
        }
    }
}

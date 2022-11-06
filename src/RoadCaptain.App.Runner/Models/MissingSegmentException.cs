using System;

namespace RoadCaptain.App.Runner.Models
{
    public class MissingSegmentException : Exception
    {
        public MissingSegmentException(string segmentId)
            : base("A segment of the route does not exist in this world")
        {
            Data.Add(nameof(segmentId), segmentId);
        }
    }
}
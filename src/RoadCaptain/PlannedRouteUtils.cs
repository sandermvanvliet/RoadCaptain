using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RoadCaptain
{
    public class PlannedRouteUtils
    {
        public static List<(Segment Climb, TrackPoint Start, TrackPoint Finish)> CalculateClimbMarkers(List<Segment> markers, ImmutableArray<TrackPoint> routePoints)
        {
            var result = new List<(Segment Climb, TrackPoint Start, TrackPoint Finish)>();
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
                            start,
                            finish: routePoints[index - 1]
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
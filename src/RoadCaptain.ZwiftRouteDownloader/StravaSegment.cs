using System.Collections.Generic;

namespace RoadCaptain.ZwiftRouteDownloader
{
    public class StravaSegment
    {
        public List<decimal[]> LatLng { get; set; }
        public List<decimal> Altitude { get; set; }
    }
}
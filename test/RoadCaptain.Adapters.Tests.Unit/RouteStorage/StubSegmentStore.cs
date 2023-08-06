using System.Collections.Generic;
using System.Linq;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters.Tests.Unit.RouteStorage
{
    public class StubSegmentStore : ISegmentStore
    {
        private readonly List<Segment> _segments;
        private readonly List<Segment> _markers;
        
        public StubSegmentStore()
        {
            var segment0Point1 = new TrackPoint(0, 0, 0, ZwiftWorldId.Watopia);
            var segment0Point2 = segment0Point1.ProjectTo(90, 100, 20);
            var segment0Point3 = segment0Point2.ProjectTo(90, 100, 20);

            var segment1Point1 = segment0Point3.ProjectTo(90, 100, 0);
            var segment1Point2 = segment1Point1.ProjectTo(90, 100, 20);
            var segment1Point3 = segment1Point2.ProjectTo(90, 100, 20);
            
            var segment2Point1 = segment1Point3.ProjectTo(90, 100, 90);
            var segment2Point2 = segment2Point1.ProjectTo(90, 100, 100);
            var segment2Point3 = segment2Point2.ProjectTo(90, 100, 90);
            
            var segment3Point1 = segment2Point3.ProjectTo(90, 100, 75);
            var segment3Point2 = segment3Point1.ProjectTo(90, 100, 70);
            var segment3Point3 = segment3Point2.ProjectTo(90, 100, 50);

            _segments = new List<Segment>
            {
                new(new List<TrackPoint>
                {
                    segment0Point1,
                    segment0Point2,
                    segment0Point3
                })
                {
                    Id = "seg-0",
                    Name = "Segment 0"
                },
                new(new List<TrackPoint>
                {
                    segment1Point1,
                    segment1Point2,
                    segment1Point3
                })
                {
                    Id = "seg-1",
                    Name = "Segment 1"
                },
                new(new List<TrackPoint>
                {
                    segment2Point1,
                    segment2Point2,
                    segment2Point3
                })
                {
                    Id = "seg-2",
                    Name = "Segment 2"
                },
                new(new List<TrackPoint>
                {
                    segment3Point1,
                    segment3Point2,
                    segment3Point3
                })
                {
                    Id = "seg-3",
                    Name = "Segment 3",
                },
            };

            foreach (var segment in _segments)
            {
                segment.Type = SegmentType.Segment;
                segment.Sport = SportType.Cycling;
                segment.CalculateDistances();
            }

            _segments[0].NextSegmentsNodeB.Add(new Turn(TurnDirection.GoStraight, "seg-1"));
            _segments[1].NextSegmentsNodeB.Add(new Turn(TurnDirection.GoStraight, "seg-2"));
            _segments[2].NextSegmentsNodeB.Add(new Turn(TurnDirection.GoStraight, "seg-3"));
            _segments[3].NextSegmentsNodeB.Add(new Turn(TurnDirection.GoStraight, "seg-0"));

            var reversedSegments = _segments
                .Select(ReverseSegment)
                .ToList();

            _segments.AddRange(reversedSegments);
            
            _markers = new List<Segment>
            {
                new(new List<TrackPoint>
                {
                    Clone(segment1Point2),
                    Clone(segment1Point3),
                    Clone(segment2Point1),
                    Clone(segment2Point2)
                })
                {
                    Id = "climb-1",
                    Name = "Climb 1",
                    Type = SegmentType.Climb,
                    Sport = SportType.Cycling
                },
                new(new List<TrackPoint>
                {
                    Clone(segment2Point2),
                    Clone(segment2Point1),
                    Clone(segment1Point3),
                    Clone(segment1Point2),
                })
                {
                    Id = "climb-1-rev",
                    Name = "Climb 1 reverse"
                }
            };

            foreach (var marker in _markers)
            {
                marker.Type = SegmentType.Climb;
                marker.Sport = SportType.Cycling;
                marker.CalculateDistances();
            }
        }
        

        private static Segment ReverseSegment(Segment input)
        {
            var reverseSegment = new Segment(
                input.Points.AsEnumerable().Reverse().Select(Clone).ToList())
            {
                Id = input.Id + "-rev",
                Name = input.Name + " reverse",
                Type = input.Type,
                Sport = input.Sport
            };
            
            reverseSegment.CalculateDistances();

            return reverseSegment;
        }

        private static TrackPoint Clone(TrackPoint input)
        {
            return new TrackPoint(input.Latitude, input.Longitude, input.Altitude, input.WorldId);
        }
        
        public List<Segment> LoadSegments(World world, SportType sport)
        {
            return _segments;
        }

        public List<Segment> LoadMarkers(World world)
        {
            return _markers;
        }
    }
}
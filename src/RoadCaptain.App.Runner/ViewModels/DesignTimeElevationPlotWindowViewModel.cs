using System.Collections.Generic;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner.ViewModels
{
    public class DesignTimeElevationPlotWindowViewModel : ElevationPlotWindowViewModel
    {
        public DesignTimeElevationPlotWindowViewModel() 
            : base(new StubSegmentStore(), new DesignTimeWindowService(), new DummyUserPreferences
            {
                ElevationProfileZoomOnPosition = true
            })
        {
            var plannedRoute = new PlannedRoute
            {
                World = new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia, Name = "Watopia"},
                WorldId = "watopia",
                Sport = SportType.Cycling,
                ZwiftRouteName = "Test Zwift Route"
            };
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("segment-1", SegmentDirection.AtoB, SegmentSequenceType.Regular));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("segment-2", SegmentDirection.AtoB, SegmentSequenceType.Regular));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("segment-3", SegmentDirection.AtoB, SegmentSequenceType.Regular));
            
            UpdateRoute(plannedRoute);
            UpdateGameState(
                new OnRouteState(
                    123,
                    456, 
                    Segments![1].Points[2], 
                    Segments![1],
                    plannedRoute,
                    SegmentDirection.AtoB, 
                    0,
                    0,
                    0));
        }
    }

    public class StubSegmentStore : ISegmentStore
    {
        private readonly List<Segment> _segments;
        private readonly List<Segment> _markers;
        
        public StubSegmentStore()
        {
            var segment1Point1 = new TrackPoint(0, 0, 0, ZwiftWorldId.Watopia);
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
                    segment1Point1,
                    segment1Point2,
                    segment1Point3
                })
                {
                    Id = "segment-1",
                    Name = "Segment 1",
                    Type = SegmentType.Segment,
                    Sport = SportType.Cycling
                },
                new(new List<TrackPoint>
                {
                    segment2Point1,
                    segment2Point2,
                    segment2Point3
                })
                {
                    Id = "segment-2",
                    Name = "Segment 2",
                    Type = SegmentType.Segment,
                    Sport = SportType.Cycling
                },
                new(new List<TrackPoint>
                {
                    segment3Point1,
                    segment3Point2,
                    segment3Point3
                })
                {
                    Id = "segment-3",
                    Name = "Segment 3",
                    Type = SegmentType.Segment,
                    Sport = SportType.Cycling
                },
            };

            foreach (var segment in _segments)
            {
                var index = 0;
                foreach (var trackPoint in segment.Points)
                {
                    trackPoint.Segment = segment;
                    trackPoint.Index = index++;
                }

                segment.CalculateDistances();
            }

            var climb1Point1 = segment1Point2.Clone();
            var climb1Point2 = segment1Point3.Clone();
            var climb1Point3 = segment2Point1.Clone();
            var climb1Point4 = segment2Point2.Clone();

            _markers = new List<Segment>
            {
                new(new List<TrackPoint>
                {
                    climb1Point1,
                    climb1Point2,
                    climb1Point3,
                    climb1Point4
                })
                {
                    Id = "climb-1",
                    Name = "Climb 1",
                    Type = SegmentType.Climb,
                    Sport = SportType.Cycling
                }
            };

            foreach (var segment in _markers)
            {
                var index = 0;
                foreach (var trackPoint in segment.Points)
                {
                    trackPoint.Segment = segment;
                    trackPoint.Index = index++;
                }

                segment.CalculateDistances();
            }
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
using System.Collections.Generic;
using RoadCaptain.Ports;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

namespace RoadCaptain.Adapters.Tests.Unit.RouteStorage
{
    public class WhenStoringRoutesInLocalDirectory
    {
        [Fact]
        public void GivenDirectoryDoesNotExist_CallingIsAvailable_DirectoryIsCreated()
        {
            var userDataDirectory = Path.GetTempPath();
            var repository = new TestableLocalDirectoryRouteRepository(userDataDirectory);

            repository.IsAvailableAsync().GetAwaiter().GetResult();

            repository
                .Directories
                .Should()
                .Contain(dir => dir == Path.Combine(userDataDirectory, "Routes"));
        }

        [Fact]
        public void GivenPlannedRoute_StoringSerializesMetadata()
        {
            var userDataDirectory = Path.GetTempPath();
            var repository = new TestableLocalDirectoryRouteRepository(userDataDirectory);
            var segments = CreateSegments();
            var plannedRoute = CreatePlannedRoute("segment-1", "segment-2", "segment-3");

            repository.StoreAsync(plannedRoute, null, segments).GetAwaiter().GetResult();

            var routes = repository.SearchAsync().GetAwaiter().GetResult();

            routes.Should().HaveCount(1);
            var route = routes.Single();
            route.Ascent.Should().Be(100);
            route.Descent.Should().Be(50);
            route.Distance.Should().Be(0.8m);
        }

        private static List<Segment> CreateSegments()
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

            var segments = new List<Segment>
            {
                new(new List<TrackPoint>
                {
                    segment1Point1,
                    segment1Point2,
                    segment1Point3
                })
                {
                    Id = "segment-1",
                    Name = "Segment 1"
                },
                new(new List<TrackPoint>
                {
                    segment2Point1,
                    segment2Point2,
                    segment2Point3
                })
                {
                    Id = "segment-2",
                    Name = "Segment 2"
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
                },
            };

            foreach (var segment in segments)
            {
                segment.Type = SegmentType.Segment;
                segment.Sport = SportType.Cycling;
                segment.CalculateDistances();
            }

            return segments;
        }
        
        private static PlannedRoute CreatePlannedRoute(params string[] segmentIds)
        {
            var plannedRoute = new PlannedRoute
            {
                World = new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia, Name = "Watopia" },
                WorldId = "watopia",
                Sport = SportType.Cycling,
                ZwiftRouteName = "Test Zwift Route",
                Name = "RoadCaptain Test Route"
            };

            foreach (var segmentId in segmentIds)
            {
                plannedRoute.RouteSegmentSequence.Add(new SegmentSequence(segmentId, SegmentDirection.AtoB,
                    SegmentSequenceType.Regular));
            }

            return plannedRoute;
        }
    }

    internal class TestableLocalDirectoryRouteRepository : LocalDirectoryRouteRepository
    {
        public TestableLocalDirectoryRouteRepository(string userDataDirectory) 
            : base(
                new LocalDirectoryRouteRepositorySettings(new StubPathProvider(userDataDirectory)), 
                new NopMonitoringEvents(), 
                new RouteStoreToDisk(new SegmentStore(), new WorldStoreToDisk()))
        {
        }

        public List<string> Directories { get; } = new();
        public Dictionary<string, string> StoredFiles { get; } = new();

        protected override bool DirectoryExists(string settingsDirectory)
        {
            return Directories.Contains(settingsDirectory);
        }

        protected override void CreateDirectory(string settingsDirectory)
        {
            Directories.Add(settingsDirectory);
        }

        protected override Task WriteAllTextAsync(string path, string serialized)
        {
            StoredFiles.Add(path, serialized);

            return Task.CompletedTask;
        }

        protected override Task<string> ReadAllTextAsync(string file)
        {
            if (StoredFiles.TryGetValue(file, out var text))
            {
                return Task.FromResult(text);
            }

            throw new FileNotFoundException();
        }
    }
    public class StubPathProvider : IPathProvider
    {
        private readonly string _userDataDirectory;

        public StubPathProvider(string userDataDirectory)
        {
            _userDataDirectory = userDataDirectory;
        }

        public string GetUserDataDirectory()
        {
            return _userDataDirectory;
        }
    }
}

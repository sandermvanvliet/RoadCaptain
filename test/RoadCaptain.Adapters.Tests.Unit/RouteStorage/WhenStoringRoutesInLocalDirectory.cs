// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
        public async Task GivenDirectoryDoesNotExist_CallingIsAvailable_DirectoryIsCreated()
        {
            var userDataDirectory = Path.GetTempPath();
            var repository = new TestableLocalDirectoryRouteRepository(userDataDirectory);

            await repository.IsAvailableAsync();

            repository
                .Directories
                .Should()
                .Contain(dir => dir == Path.Combine(userDataDirectory, "Routes"));
        }
    }

    internal class TestableLocalDirectoryRouteRepository : LocalDirectoryRouteRepository
    {
        public TestableLocalDirectoryRouteRepository(string userDataDirectory) 
            : base(
                new LocalDirectoryRouteRepositorySettings(new StubPathProvider(userDataDirectory)), 
                new NopMonitoringEvents(), 
                new RouteStoreToDisk(new SegmentStore(new NopMonitoringEvents()), new WorldStoreToDisk()))
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

        protected override string[] GetFilesFromDirectory()
        {
            return StoredFiles.Keys.ToArray();
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

        public string? RouteBuilderExecutable()
        {
            throw new System.NotImplementedException();
        }
    }
}


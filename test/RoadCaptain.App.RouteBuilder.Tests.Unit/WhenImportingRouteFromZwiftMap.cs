using System;
using FluentAssertions;
using RoadCaptain.Adapters;
using Xunit;
using RoadCaptain.App.RouteBuilder.Models;
using RoadCaptain.App.RouteBuilder.UseCases;

namespace RoadCaptain.App.RouteBuilder.Tests.Unit
{
    public class WhenImportingRouteFromZwiftMap
    {
        [Fact]
        public void GivenZwiftMapRoute_ItIsMappedToAPlannedRoute()
        {
            var fileRoot = Environment.CurrentDirectory;
            var segmentStore = new SegmentStore(fileRoot);
            var worldStore = new WorldStoreToDisk(fileRoot);
            var routeStore = new RouteStoreToDisk(segmentStore, worldStore);

            var expectedPlannedRoute = routeStore.LoadFrom("ImportedFromZwiftMap.json");

            var useCase = new ConvertZwiftMapRouteUseCase(worldStore, segmentStore);

            var result = useCase.Execute(ZwiftMapRoute.FromGpxFile("zwiftmap-route.gpx"));

            routeStore.Store(result, @"c:\temp\result.json");
            result.Should().BeEquivalentTo(expectedPlannedRoute);
        }
    }
}


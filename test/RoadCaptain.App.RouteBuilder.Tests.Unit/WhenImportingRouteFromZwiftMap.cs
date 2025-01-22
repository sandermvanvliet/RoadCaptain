// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;
using FluentAssertions;
using RoadCaptain.Adapters;
using Xunit;
using RoadCaptain.App.RouteBuilder.Models;
using RoadCaptain.UseCases;

namespace RoadCaptain.App.RouteBuilder.Tests.Unit
{
    public class WhenImportingRouteFromZwiftMap
    {
        [Fact]
        public async Task GivenZwiftMapRoute_ItIsMappedToAPlannedRoute()
        {
            var fileRoot = Environment.CurrentDirectory;
            var segmentStore = new SegmentStore(fileRoot, new Shared.NopMonitoringEvents());
            var worldStore = new WorldStoreToDisk(fileRoot);
            var routeStore = new RouteStoreToDisk(segmentStore, worldStore);

            var expectedPlannedRoute = routeStore.LoadFrom("ImportedFromZwiftMap.json");
            // We don't do loop detection on importing from Zwiftmap (yet anyway)
            expectedPlannedRoute.LoopMode = LoopMode.Unknown;

            var useCase = new ConvertZwiftMapRouteUseCase(worldStore, segmentStore);

            var result = useCase.Execute(ZwiftMapRoute.FromGpxFile("zwiftmap-route.gpx"));

            await routeStore.StoreAsync(result, @"c:\temp\result.json");
            result.Should().BeEquivalentTo(expectedPlannedRoute);
        }
    }
}



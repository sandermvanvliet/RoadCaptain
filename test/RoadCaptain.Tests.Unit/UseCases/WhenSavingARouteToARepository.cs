// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.Commands;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.Tests.Unit.UseCases
{
    public class WhenSavingARouteToARepository
    {
        [Fact]
        public void GivenUnknownRepositoryName_ExceptionIsThrown()
        {
            var command = new SaveRouteCommand(new PlannedRoute(), "NOT IMPORTANT", "REPOSITORY DOES NOT EXIST", "NOT IMPORTANT", null);
            var useCase = new SaveRouteUseCase(Array.Empty<IRouteRepository>(), new SegmentStore(new NopMonitoringEvents()), new StubRouteStore());

            var action = () => useCase.ExecuteAsync(command).GetAwaiter().GetResult();

            action
                .Should()
                .Throw<Exception>()
                .Which
                .Message
                .Should()
                .Be("Unable to find a repository with the name 'REPOSITORY DOES NOT EXIST'");
        }

        [Fact]
        public void GivenCommandHasNoRepositoryNameAndNoFileName_ExceptionIsThrown()
        {
            var command = new SaveRouteCommand(new PlannedRoute(), "NOT IMPORTANT", null, null, null);
            var useCase = new SaveRouteUseCase(Array.Empty<IRouteRepository>(), new SegmentStore(new NopMonitoringEvents()), new StubRouteStore());

            var action = () => useCase.ExecuteAsync(command).GetAwaiter().GetResult();

            action
                .Should()
                .Throw<Exception>()
                .Which
                .Message
                .Should()
                .Be("Neither a repository name or a local file was provided and I can't save the route because I need one of those");
        }

        [Fact]
        public async Task GivenCommandHasOnlyFileNameSpecified_RouteIsSavedToDisk()
        {
            var command = new SaveRouteCommand(new PlannedRoute(), "NOT IMPORTANT", null, "c:\\temp\\route.json", null);
            var stubRouteStore = new StubRouteStore();
            var useCase = new SaveRouteUseCase(Array.Empty<IRouteRepository>(), new SegmentStore(new NopMonitoringEvents()), stubRouteStore);

            await useCase.ExecuteAsync(command);

            stubRouteStore
                .StoredRoutes
                .Should()
                .ContainKey("c:\\temp\\route.json");
        }

        [Fact]
        public async Task GivenCommandHasRepositoryNameAndFileName_RouteIsSavedToRepository()
        {
            var command = new SaveRouteCommand(new PlannedRoute(), "NOT IMPORTANT", "TEST", "c:\\temp\\route.json", null);
            var stubRouteStore = new StubRouteStore();
            var stubRepository = new StubRepository();
            var useCase = new SaveRouteUseCase(new[] { stubRepository }, new StubSegmentStore(), stubRouteStore);

            await useCase.ExecuteAsync(command);

            stubRepository
                .StoredRoutes
                .Should()
                .HaveCount(1);
        }
    }
}

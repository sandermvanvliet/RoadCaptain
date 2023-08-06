// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
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
            var command = new SaveRouteCommand(new PlannedRoute(), "NOT IMPORTANT", "REPOSITORY DOES NOT EXIST", "NOT IMPORTANT", "NOT IMPORTANT");
            var useCase = new SaveRouteUseCase(Array.Empty<IRouteRepository>(), new SegmentStore(), new StubRouteStore());

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
            var command = new SaveRouteCommand(new PlannedRoute(), "NOT IMPORTANT", null, "NOT IMPORTANT", null);
            var useCase = new SaveRouteUseCase(Array.Empty<IRouteRepository>(), new SegmentStore(), new StubRouteStore());

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
        public void GivenCommandHasOnlyFileNameSpecified_RouteIsSavedToDisk()
        {
            var command = new SaveRouteCommand(new PlannedRoute(), "NOT IMPORTANT", null, "NOT IMPORTANT", "c:\\temp\\route.json");
            var stubRouteStore = new StubRouteStore();
            var useCase = new SaveRouteUseCase(Array.Empty<IRouteRepository>(), new SegmentStore(), stubRouteStore);

            useCase.ExecuteAsync(command).GetAwaiter().GetResult();

            stubRouteStore
                .StoredRoutes
                .Should()
                .ContainKey("c:\\temp\\route.json");
        }

        [Fact]
        public void GivenCommandHasRepositoryNameAndFileName_RouteIsSavedToRepository()
        {
            var command = new SaveRouteCommand(new PlannedRoute(), "NOT IMPORTANT", "TEST", "NOT IMPORTANT", "c:\\temp\\route.json");
            var stubRouteStore = new StubRouteStore();
            var stubRepository = new StubRepository();
            var useCase = new SaveRouteUseCase(new[] { stubRepository }, new StubSegmentStore(), stubRouteStore);

            useCase.ExecuteAsync(command).GetAwaiter().GetResult();

            stubRepository
                .StoredRoutes
                .Should()
                .HaveCount(1);
        }
    }
}

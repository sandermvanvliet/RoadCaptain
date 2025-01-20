// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;
using FluentAssertions;
using RoadCaptain.Commands;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.Tests.Unit.UseCases
{
    public class WhenSearchingForRoutes
    {
        [Fact]
        public async Task GivenSpecificRepositoryNameAndItDoesNotExist_RepositoryIsIgnored()
        {
            var command = new SearchRouteCommand("REPOSITORY DOES NOT EXIST");
            var useCase = new SearchRoutesUseCase(new[] { new StubRepository() }, new NopMonitoringEvents());

            var results = await useCase.ExecuteAsync(command);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GivenThreeRepositoriesEachWithOneRouteAndSearchingAllRepositories_ThreeRoutesAreReturned()
        {
            var command = new SearchRouteCommand("All");
            var useCase = new SearchRoutesUseCase(new[] { new StubRepository("One"), new StubRepository("Two"), new StubRepository("Three") }, new NopMonitoringEvents());

            var result = await useCase.ExecuteAsync(command);

            result
                .Should()
                .HaveCount(3);
        }

        [Fact]
        public async Task GivenThreeRepositoriesEachWithOneRouteAndSearchingInSpecificRepository_SingleRouteIsReturned()
        {
            var command = new SearchRouteCommand("Two");
            var useCase = new SearchRoutesUseCase(new[] { new StubRepository("One"), new StubRepository("Two"), new StubRepository("Three") }, new NopMonitoringEvents());

            var result = await useCase.ExecuteAsync(command);

            result
                .Should()
                .HaveCount(1);
        }

        [Fact]
        public async Task GivenThreeRepositoriesEachWithOneRouteAndSearchingAllRepositoriesWhereOneRepositoryThrowsException_TwoRoutesAreReturned()
        {
            var command = new SearchRouteCommand("All");
            var useCase = new SearchRoutesUseCase(new[] { new StubRepository("One"), new StubRepository("Two", throwsOnSearch: true), new StubRepository("Three") }, new NopMonitoringEvents());

            var result = await useCase.ExecuteAsync(command);

            result
                .Should()
                .HaveCount(2);
        }
    }
}


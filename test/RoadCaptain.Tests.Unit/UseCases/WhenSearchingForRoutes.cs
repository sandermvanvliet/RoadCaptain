// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using FluentAssertions;
using RoadCaptain.Commands;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.Tests.Unit.UseCases
{
    public class WhenSearchingForRoutes
    {
        [Fact]
        public void GivenSpecificRepositoryNameAndItDoesNotExist_RepositoryIsIgnored()
        {
            var command = new SearchRouteCommand("REPOSITORY DOES NOT EXIST");
            var useCase = new SearchRoutesUseCase(new[] { new StubRepository() }, new NopMonitoringEvents());

            var results = useCase.ExecuteAsync(command).GetAwaiter().GetResult();

            results.Should().BeEmpty();
        }

        [Fact]
        public void GivenThreeRepositoriesEachWithOneRouteAndSearchingAllRepositories_ThreeRoutesAreReturned()
        {
            var command = new SearchRouteCommand("All");
            var useCase = new SearchRoutesUseCase(new[] { new StubRepository("One"), new StubRepository("Two"), new StubRepository("Three") }, new NopMonitoringEvents());

            var result = useCase.ExecuteAsync(command).GetAwaiter().GetResult();

            result
                .Should()
                .HaveCount(3);
        }

        [Fact]
        public void GivenThreeRepositoriesEachWithOneRouteAndSearchingInSpecificRepository_SingleRouteIsReturned()
        {
            var command = new SearchRouteCommand("Two");
            var useCase = new SearchRoutesUseCase(new[] { new StubRepository("One"), new StubRepository("Two"), new StubRepository("Three") }, new NopMonitoringEvents());

            var result = useCase.ExecuteAsync(command).GetAwaiter().GetResult();

            result
                .Should()
                .HaveCount(1);
        }

        [Fact]
        public void GivenThreeRepositoriesEachWithOneRouteAndSearchingAllRepositoriesWhereOneRepositoryThrowsException_TwoRoutesAreReturned()
        {
            var command = new SearchRouteCommand("All");
            var useCase = new SearchRoutesUseCase(new[] { new StubRepository("One"), new StubRepository("Two", throwsOnSearch: true), new StubRepository("Three") }, new NopMonitoringEvents());

            var result = useCase.ExecuteAsync(command).GetAwaiter().GetResult();

            result
                .Should()
                .HaveCount(2);
        }
    }
}


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
        public void GivenSpecificRepositoryNameAndItDoesNotExist_ExceptionIsThrown()
        {
            var command = new SearchRouteCommand("REPOSITORY DOES NOT EXIST");
            var useCase = new SearchRoutesUseCase(new[] { new StubRepository() }, new NopMonitoringEvents());

            var action = () => useCase.ExecuteAsync(command).GetAwaiter().GetResult();

            action
                .Should()
                .Throw<Exception>()
                .Which
                .Message
                .Should()
                .Be("Could not find a route repository with the name 'REPOSITORY DOES NOT EXIST'");
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

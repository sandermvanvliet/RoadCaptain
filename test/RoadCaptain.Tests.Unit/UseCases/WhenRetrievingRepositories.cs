using System;
using FluentAssertions;
using RoadCaptain.Commands;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;
using Xunit;

namespace RoadCaptain.Tests.Unit.UseCases
{
    public class WhenRetrievingRepositories
    {
        [Fact]
        public void GivenNoIntent_ArgumentExceptionIsThrown()
        {
            var command = new RetrieveRepositoryNamesCommand(RetrieveRepositoriesIntent.Unknown);
            var useCase = new RetrieveRepositoryNamesUseCase(Array.Empty<IRouteRepository>());

            var action = () => useCase.Execute(command);

            action
                .Should()
                .Throw<ArgumentException>()
                .Which
                .Message
                .Should()
                .Be("Invalid intent");
        }

        [Fact]
        public void GivenIntentIsRetrieve_ListOfRepositoriesContainsRepositoryNameAll()
        {
            var command = new RetrieveRepositoryNamesCommand(RetrieveRepositoriesIntent.Retrieve);
            var useCase = new RetrieveRepositoryNamesUseCase(Array.Empty<IRouteRepository>());

            useCase
                .Execute(command)
                .Should()
                .Contain("All");
        }

        [Fact]
        public void GivenIntentIsStore_ListOfRepositoriesDoesContainRepositoryNameAll()
        {
            var command = new RetrieveRepositoryNamesCommand(RetrieveRepositoriesIntent.Store);
            var useCase = new RetrieveRepositoryNamesUseCase(Array.Empty<IRouteRepository>());

            useCase
                .Execute(command)
                .Should()
                .NotContain("All");
        }
    }
}
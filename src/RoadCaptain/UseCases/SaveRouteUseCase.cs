using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoadCaptain.Commands;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class SaveRouteUseCase
    {
        private readonly IEnumerable<IRouteRepository> _repositories;

        public SaveRouteUseCase(IEnumerable<IRouteRepository> repositories)
        {
            _repositories = repositories;
        }
        
        public async Task ExecuteAsync(SaveRouteCommand saveRouteCommand)
        {
            var repository = _repositories.SingleOrDefault(r => r.Name == saveRouteCommand.RepositoryName);

            if (repository == null)
            {
                throw new Exception(
                    $"Unable to find a repository with the name '{saveRouteCommand.RepositoryName}");
            }

            saveRouteCommand.Route.Name = saveRouteCommand.RouteName;
            
            var token = new OAuthToken();
            
            await repository.StoreAsync(saveRouteCommand.Route, token);
        }
    }
}
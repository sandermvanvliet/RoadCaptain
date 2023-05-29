using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoadCaptain.Commands;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class SearchRoutesUseCase
    {
        private readonly IEnumerable<IRouteRepository> _routeRepositories;

        public SearchRoutesUseCase(IEnumerable<IRouteRepository> routeRepositories)
        {
            _routeRepositories = routeRepositories;
        }
        
        public async Task<IEnumerable<RouteModel>> ExecuteAsync(SearchRouteCommand command)
        {
            var repositoriesToSearch = new List<IRouteRepository>();
            
            if (!"all".Equals(command.Repository, StringComparison.InvariantCultureIgnoreCase))
            {
                var match = _routeRepositories.SingleOrDefault(r =>
                    r.Name.Equals(command.Repository, StringComparison.InvariantCultureIgnoreCase));

                if (match != null)
                {
                    repositoriesToSearch.Add(match);
                }
                else
                {
                    throw new Exception($"Could not find a route repository with the name '{command.Repository}'");
                }
            }
            else
            {
                repositoriesToSearch.AddRange(_routeRepositories);
            }

            var tasks = new List<Task<RouteModel[]>>();
            
            foreach (var repository in repositoriesToSearch)
            {
                tasks.Add(repository.SearchAsync());
            }

            await Task.WhenAll(tasks);

            return tasks
                .SelectMany(t => t.Result)
                .ToArray();
        }
    }
}
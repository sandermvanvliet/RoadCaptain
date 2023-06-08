// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
using RoadCaptain.Commands;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class RetrieveRepositoryNamesUseCase
    {
        private readonly IEnumerable<IRouteRepository> _routeRepositories;

        public RetrieveRepositoryNamesUseCase(IEnumerable<IRouteRepository> routeRepositories)
        {
            _routeRepositories = routeRepositories;
        }

        public string[] Execute(RetrieveRepositoryNameCommand command)
        {
            var repositories = _routeRepositories.Select(r => r.Name);
            
            if (command.Intent == RetrieveRepositoriesIntent.Retrieve)
            {
                repositories = new[] { "All" }
                    .Concat(repositories);
            }

            return repositories.ToArray();
        }
    }
}

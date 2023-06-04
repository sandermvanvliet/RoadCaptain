// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
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

        public string[] Execute()
        {
            return new [] { "All" }
                .Concat(_routeRepositories.Select(r => r.Name))
                .ToArray();
        }
    }
}

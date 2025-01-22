// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
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

        public string[] Execute(RetrieveRepositoryNamesCommand command)
        {
            var repositories = command.Intent switch
            {
                RetrieveRepositoriesIntent.Retrieve => new[] { "All" }.Concat(_routeRepositories.Select(r => r.Name)).ToArray(),
                RetrieveRepositoriesIntent.Manage => new [] { "All"}.Concat(_routeRepositories.Where(r => !r.IsReadOnly).Select(r => r.Name)).ToArray(),
                RetrieveRepositoriesIntent.Store => _routeRepositories.Where(r => !r.IsReadOnly).Select(r => r.Name).ToArray(),
                _ => throw new ArgumentException("Invalid intent")
            };

            return repositories;
        }
    }
}

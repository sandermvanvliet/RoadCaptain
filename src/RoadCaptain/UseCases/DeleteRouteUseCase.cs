// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using RoadCaptain.Commands;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class DeleteRouteUseCase
    {
        private readonly ImmutableList<IRouteRepository> _routeRepositories;

        public DeleteRouteUseCase(IEnumerable<IRouteRepository> routeRepositories)
        {
            _routeRepositories = routeRepositories.ToImmutableList();
        }
        
        public async Task ExecuteAsync(DeleteRouteCommand deleteRouteCommand)
        {
            var routeRepository = _routeRepositories.SingleOrDefault(r => r.Name == deleteRouteCommand.RepositoryName);

            if (routeRepository == null)
            {
                throw new ArgumentException(
                    "Attempted to delete a route on a repository that I don't know about. Can't delete this route.");
            }
            
            await routeRepository.DeleteAsync(deleteRouteCommand.RouteUri);
        }
    }
}

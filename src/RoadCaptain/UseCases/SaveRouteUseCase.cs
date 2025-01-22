// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
        private readonly ISegmentStore _segmentStore;
        private readonly IRouteStore _routeStore;

        public SaveRouteUseCase(IEnumerable<IRouteRepository> repositories, ISegmentStore segmentStore, IRouteStore routeStore)
        {
            _repositories = repositories;
            _segmentStore = segmentStore;
            _routeStore = routeStore;
        }
        
        public async Task<Uri> ExecuteAsync(SaveRouteCommand saveRouteCommand)
        {
            if (!string.IsNullOrEmpty(saveRouteCommand.RepositoryName))
            {
                var repository = _repositories.SingleOrDefault(r => r.Name == saveRouteCommand.RepositoryName);

                if (repository == null)
                {
                    throw new Exception(
                        $"Unable to find a repository with the name '{saveRouteCommand.RepositoryName}'");
                }

                saveRouteCommand.Route.Name = saveRouteCommand.RouteName;

                var segments = _segmentStore.LoadSegments(saveRouteCommand.Route.World!, saveRouteCommand.Route.Sport);

                // Ensure we do this just before saving so that we have accurate information
                saveRouteCommand.Route.CalculateMetrics(segments);

                return (await repository.StoreAsync(saveRouteCommand.Route, saveRouteCommand.RouteUri)).Uri!;
            }

            if (!string.IsNullOrEmpty(saveRouteCommand.OutputFilePath))
            {
                return await _routeStore.StoreAsync(saveRouteCommand.Route, saveRouteCommand.OutputFilePath);
            }

            throw new ArgumentException(
                "Neither a repository name or a local file was provided and I can't save the route because I need one of those");
        }
    }
}

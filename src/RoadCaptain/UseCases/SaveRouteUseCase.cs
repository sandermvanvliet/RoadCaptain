// Copyright (c) 2023 Sander van Vliet
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

        public SaveRouteUseCase(IEnumerable<IRouteRepository> repositories, ISegmentStore segmentStore)
        {
            _repositories = repositories;
            _segmentStore = segmentStore;
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
            
            var segments = _segmentStore.LoadSegments(saveRouteCommand.Route.World!, saveRouteCommand.Route.Sport);
            
            await repository.StoreAsync(saveRouteCommand.Route, saveRouteCommand.Token, segments);
        }
    }
}

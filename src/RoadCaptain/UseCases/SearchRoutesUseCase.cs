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
    public class SearchRoutesUseCase
    {
        private readonly IEnumerable<IRouteRepository> _routeRepositories;
        private readonly MonitoringEvents _monitoringEvents;

        public SearchRoutesUseCase(IEnumerable<IRouteRepository> routeRepositories, MonitoringEvents monitoringEvents)
        {
            _routeRepositories = routeRepositories;
            _monitoringEvents = monitoringEvents;
        }
        
        public async Task<IEnumerable<RouteModel>> ExecuteAsync(SearchRouteCommand command)
        {
            var repositoriesToSearch = new List<IRouteRepository>();

            if (command.Repositories.Length == 1 &&
                "all".Equals(command.Repositories[0], StringComparison.InvariantCultureIgnoreCase))
            {
                repositoriesToSearch.AddRange(_routeRepositories);
            }
            else
            {
                repositoriesToSearch = command
                    .Repositories
                    .Select(r => _routeRepositories.SingleOrDefault(rep => rep.Name.Equals(r)))
                    .Where(r => r != null)
                    .Select(r => r!)
                    .ToList();
            }

            var tasks = new List<Task<RouteModel[]>>();
            
            foreach (var repository in repositoriesToSearch)
            {
                tasks.Add(repository.SearchAsync(
                    "all".Equals(command.World, StringComparison.InvariantCultureIgnoreCase) ? null : command.World,
                    command.Creator,
                    command.Name,
                    command.ZwiftRouteName,
                    command.MinDistance,
                    command.MaxDistance,
                    command.MinAscent,
                    command.MaxAscent,
                    command.MinDescent,
                    command.MaxDescent,
                    command.IsLoop,
                    command.KomSegments,
                    command.SprintSegments));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch 
            {
                var failedTasks = tasks
                    .Where(t => t.IsFaulted || t.IsCanceled)
                    .ToArray();

                foreach (var failedTask in failedTasks)
                {
                    if (failedTask.IsCanceled)
                    {
                        _monitoringEvents.Warning("Unable to retrieve routes from repository because the repository timed-out");
                    }
                    else if(failedTask.Exception != null)
                    {
                        _monitoringEvents.Error(failedTask.Exception, "Unable to retrieve routes from repository");
                    }
                    else
                    {
                        _monitoringEvents.Error("Unable to retrieve routes from repository");
                    }
                }
            }

            return tasks
                .Where(t => t.IsCompletedSuccessfully)
                .SelectMany(t => t.Result)
                .ToArray();
        }
    }
}

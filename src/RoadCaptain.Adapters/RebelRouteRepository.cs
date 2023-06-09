// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class RebelRouteRepository : IRouteRepository
    {
        private static readonly JsonSerializerSettings JsonSettings = new();
        private readonly JsonSerializer _serializer;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly RouteStoreToDisk _routeStoreToDisk;

        public RebelRouteRepository(MonitoringEvents monitoringEvents, RouteStoreToDisk routeStoreToDisk)
        {
            _monitoringEvents = monitoringEvents;
            _routeStoreToDisk = routeStoreToDisk;
            _serializer = JsonSerializer.Create(JsonSettings);
        }
        
        public Task<bool> IsAvailableAsync()
        {
            return Task.FromResult(true);
        }

        public async Task<RouteModel[]> SearchAsync(string? world = null, string? creator = null, string? name = null,
            string? zwiftRouteName = null,
            int? minDistance = null, int? maxDistance = null, int? minAscent = null, int? maxAscent = null,
            int? minDescent = null, int? maxDescent = null, bool? isLoop = null, string[]? komSegments = null,
            string[]? sprintSegments = null)
        {
            var path = Path.GetDirectoryName(GetType().Assembly.Location);

            var routeFiles = Directory.GetFiles(Path.Combine(path, "Routes"), "RebelRoute-*.json", SearchOption.TopDirectoryOnly);
            
            var routeModels = new List<RouteModel>();

            foreach (var file in routeFiles)
            {
                try
                {
                    var serialized = File.ReadAllText(file);
                    var plannedRoute = UpgradeIfNecessaryAndSerialize(serialized);

                    var routeModel = new RouteModel
                    {
                        RepositoryName = Name,
                        Uri = new Uri($"https://zwiftinsider.com/rebel-routes/{plannedRoute.Name}"),
                        Name = plannedRoute.Name,
                        ZwiftRouteName = plannedRoute.ZwiftRouteName,
                        CreatorName = "Zwift Insider",
                        IsLoop = plannedRoute.IsLoop,
                        PlannedRoute = plannedRoute,
                        World = plannedRoute.WorldId
                    };
                    
                    routeModels.Add(routeModel);
                }
                catch (Exception e)
                {
                    _monitoringEvents.Error(e, "Unable to deserialize route model from file '{File}'", file);
                }
            }
            
            var query = routeModels.AsQueryable();

            if (!string.IsNullOrEmpty(world))
            {
                query = query.Where(route => string.Equals(route.World, world, StringComparison.InvariantCultureIgnoreCase));
            }
            
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(route => string.Equals(route.Name, name, StringComparison.InvariantCultureIgnoreCase));
            }
            
            if (!string.IsNullOrEmpty(zwiftRouteName))
            {
                query = query.Where(route =>
                    string.Equals(route.ZwiftRouteName, zwiftRouteName, StringComparison.InvariantCultureIgnoreCase));
            }

            if (minDistance is > 0)
            {
                query = query.Where(route => route.Distance >= minDistance.Value);
            }

            if (minAscent is > 0)
            {
                query = query.Where(route => route.Ascent >= minAscent.Value);
            }

            if (minDescent is > 0)
            {
                query = query.Where(route => route.Descent >= minDescent.Value);
            }

            if (maxDistance is > 0)
            {
                query = query.Where(route => route.Distance <= maxDistance.Value);
            }

            if (maxAscent is > 0)
            {
                query = query.Where(route => route.Ascent <= maxAscent.Value);
            }


            if (maxDescent is > 0)
            {
                query = query.Where(route => route.Descent <= maxDescent.Value);
            }

            if (isLoop is { })
            {
                query = query.Where(route => route.IsLoop == isLoop);
            }

            return query.ToArray();
        }

        public Task<RouteModel> StoreAsync(PlannedRoute plannedRoute, string? token, List<Segment> segments)
        {
            throw new InvalidOperationException("Rebel route repository is read-only");
        }

        public string Name => "Zwift Insider - Rebel Routes";

        private PlannedRoute? UpgradeIfNecessaryAndSerialize(string? routeModelSerialized)
        {
            if (routeModelSerialized == null)
            {
                return null;
            }

            try
            {
                return _routeStoreToDisk.DeserializeAndUpgrade(routeModelSerialized);
            }
            catch
            {
                return null;
            }
        }
    }
}

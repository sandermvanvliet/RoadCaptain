using System;
using System.Collections.Generic;
using System.IO;
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

        public async Task<RouteModel[]> SearchAsync(string? world = null, string? creator = null, string? name = null, string? zwiftRouteName = null,
            decimal? minDistance = null, decimal? maxDistance = null, decimal? minAscent = null, decimal? maxAscent = null,
            decimal? minDescent = null, decimal? maxDescent = null, bool? isLoop = null, string[]? komSegments = null,
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
                    };
                    
                    routeModels.Add(routeModel);
                }
                catch (Exception e)
                {
                    _monitoringEvents.Error(e, "Unable to deserialize route model from file '{File}'", file);
                }
            }

            return routeModels.ToArray();
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
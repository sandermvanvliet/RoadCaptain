using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class LocalDirectoryRouteRepository : IRouteRepository
    {
        private readonly LocalDirectoryRouteRepositorySettings _settings;
        private static readonly JsonSerializerSettings JsonSettings = new();
        private readonly JsonSerializer _serializer;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly RouteStoreToDisk _routeStoreToDisk;

        public LocalDirectoryRouteRepository(LocalDirectoryRouteRepositorySettings settings, MonitoringEvents monitoringEvents, RouteStoreToDisk routeStoreToDisk)
        {
            _settings = settings;
            _monitoringEvents = monitoringEvents;
            _routeStoreToDisk = routeStoreToDisk;
            _serializer = JsonSerializer.Create(JsonSettings);
            Name = _settings.Name;
        }

        public string Name { get; }

        public Task<bool> IsAvailableAsync()
        {
            if (!_settings.IsValid)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(Directory.Exists(_settings.Directory));
        }

        public async Task<RouteModel[]> SearchAsync(
            string? world = null,
            string? creator = null,
            string? name = null,
            string? zwiftRouteName = null,
            decimal? minDistance = null,
            decimal? maxDistance = null,
            decimal? minAscent = null,
            decimal? maxAscent = null,
            decimal? minDescent = null,
            decimal? maxDescent = null,
            bool? isLoop = null,
            string[]? komSegments = null,
            string[]? sprintSegments = null)
        {
            if (!_settings.IsValid)
            {
                throw new Exception("Route repository is not configured correctly");
            }
            
            var routeFiles = Directory.GetFiles(_settings.Directory!, "roadcaptain-route-*.json", SearchOption.TopDirectoryOnly);
            var routeModels = new List<RouteModel>();

            foreach (var file in routeFiles)
            {
                try
                {
                    using var textReader = new StreamReader(File.OpenRead(file));
                    await using var jsonTextReader = new JsonTextReader(textReader);
                    var routeModel = _serializer.Deserialize<RouteModel>(jsonTextReader);

                    if (routeModel != null)
                    {
                        routeModel.RepositoryName = Name;
                        routeModel.Uri = new Uri(file);
                        routeModel.PlannedRoute = UpgradeIfNecessaryAndSerialize(routeModel.Serialized);
                        routeModels.Add(routeModel);
                    }
                    else
                    {
                        throw new Exception("Deserialized model was null");
                    }
                }
                catch (Exception e)
                {
                    _monitoringEvents.Error(e, "Unable to deserialize route model from file '{File}'", file);
                }
            }

            return routeModels.ToArray();
        }

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
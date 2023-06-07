// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class LocalDirectoryRouteRepository : IRouteRepository
    {
        private const string FileNamePattern = "roadcaptain-route-*.json";
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

        public async Task<RouteModel[]> SearchAsync(string? world = null,
            string? creator = null,
            string? name = null,
            string? zwiftRouteName = null,
            int? minDistance = null,
            int? maxDistance = null,
            int? minAscent = null,
            int? maxAscent = null,
            int? minDescent = null,
            int? maxDescent = null,
            bool? isLoop = null,
            string[]? komSegments = null,
            string[]? sprintSegments = null)
        {
            if (!_settings.IsValid)
            {
                throw new Exception("Route repository is not configured correctly");
            }
            
            var routeFiles = Directory.GetFiles(_settings.Directory!, FileNamePattern, SearchOption.TopDirectoryOnly);
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
                        routeModel.World = routeModel.PlannedRoute?.WorldId;
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

        public async Task<RouteModel> StoreAsync(PlannedRoute plannedRoute, OAuthToken token)
        {
            var storageModel = new RouteModel
            {
                Name = plannedRoute.Name,
                World = plannedRoute.WorldId,
                ZwiftRouteName = plannedRoute.ZwiftRouteName,
                IsLoop = plannedRoute.IsLoop,
                RepositoryName = Name,
                Serialized = RouteStoreToDisk.SerializeAsJson(plannedRoute)
            };

            var routeNameForFile = storageModel.Name!.Replace(" ", "").ToLower();

            var serialized = JsonConvert.SerializeObject(storageModel, JsonSettings);
            var path = Path.Combine(_settings.Directory!, FileNamePattern.Replace("*", routeNameForFile));
            
            await File.WriteAllTextAsync(path, serialized);

            return storageModel;
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

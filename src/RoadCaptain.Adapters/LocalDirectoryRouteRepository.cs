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
    internal class LocalDirectoryRouteRepository : IRouteRepository
    {
        private const string FileNamePattern = "roadcaptain-route-*.json";
        private static readonly JsonSerializerSettings JsonSettings = new();
        private readonly MonitoringEvents _monitoringEvents;
        private readonly RouteStoreToDisk _routeStoreToDisk;
        private readonly LocalDirectoryRouteRepositorySettings _settings;

        public LocalDirectoryRouteRepository(
            LocalDirectoryRouteRepositorySettings settings,
            MonitoringEvents monitoringEvents,
            RouteStoreToDisk routeStoreToDisk)
        {
            _settings = settings;
            _monitoringEvents = monitoringEvents;
            _routeStoreToDisk = routeStoreToDisk;
            JsonSerializer.Create(JsonSettings);
            Name = _settings.Name;
        }

        public string Name { get; }
        public bool IsReadOnly => false;

        public Task<bool> IsAvailableAsync()
        {
            if (!_settings.IsValid)
            {
                return Task.FromResult(false);
            }

            if (!DirectoryExists(_settings.Directory))
            {
                CreateDirectory(_settings.Directory);
            }

            return Task.FromResult(true);
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

            if (!DirectoryExists(_settings.Directory))
            {
                CreateDirectory(_settings.Directory);
            }

            var routeFiles = GetFilesFromDirectory();
            var routeModels = new List<RouteModel>();

            foreach (var file in routeFiles)
            {
                try
                {
                    var serialized = await ReadAllTextAsync(file);
                    var routeModel = JsonConvert.DeserializeObject<RouteModel>(serialized, JsonSettings);

                    if (routeModel != null)
                    {
                        routeModel.RepositoryName = Name;
                        routeModel.Uri = new Uri(file);
                        routeModel.PlannedRoute = UpgradeIfNecessaryAndSerialize(routeModel.Serialized);
                        routeModel.World = routeModel.PlannedRoute?.WorldId;
                        routeModel.CreatorName = "You";
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
                query = query.Where(route =>
                    string.Equals(route.World, world, StringComparison.InvariantCultureIgnoreCase));
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(route =>
                    string.Equals(route.Name, name, StringComparison.InvariantCultureIgnoreCase));
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

            if (isLoop is not null)
            {
                query = query.Where(route => route.IsLoop == isLoop);
            }

            return query.ToArray();
        }

        protected virtual async Task<string> ReadAllTextAsync(string file)
        {
            return await File.ReadAllTextAsync(file);
        }

        public async Task<RouteModel> StoreAsync(PlannedRoute plannedRoute, string? token, List<Segment> segments)
        {
            if (!DirectoryExists(_settings.Directory))
            {
                CreateDirectory(_settings.Directory);
            }

            var storageModel = new RouteModel
            {
                Name = plannedRoute.Name,
                World = plannedRoute.WorldId,
                ZwiftRouteName = plannedRoute.ZwiftRouteName,
                IsLoop = plannedRoute.IsLoop,
                RepositoryName = Name,
                Serialized = RouteStoreToDisk.SerializeAsJson(plannedRoute, Formatting.None)
            };
            
            CalculateTotalAscentAndDescent(storageModel, plannedRoute, segments);

            var routeNameForFile = storageModel.Name!.Replace(" ", "").ToLower();

            var serialized = JsonConvert.SerializeObject(storageModel, JsonSettings);
            var path = Path.Combine(_settings.Directory!, FileNamePattern.Replace("*", routeNameForFile));

            await WriteAllTextAsync(path, serialized);

            return storageModel;
        }

        private static void CalculateTotalAscentAndDescent(RouteModel routeModel, PlannedRoute route, List<Segment> segments)
        {
            double totalAscent = 0;
            double totalDescent = 0;
            double totalDistance = 0;

            var pointsOnRoute = route.GetTrackPoints(segments);

            TrackPoint? previous = null;

            foreach (var point in pointsOnRoute)
            {
                if (previous == null)
                {
                    previous = point;
                    continue;
                }

                var altitudeDelta = point.Altitude - previous.Altitude;
                if (altitudeDelta > 0)
                {
                    totalAscent += altitudeDelta;
                }
                else if (altitudeDelta < 0)
                {
                    totalDescent += Math.Abs(altitudeDelta);
                }

                totalDistance += TrackPoint.GetDistanceFromLatLonInMeters(
                    previous.Latitude, 
                    previous.Longitude,
                    point.Latitude, 
                    point.Longitude);

                previous = point;
            }

            routeModel.Distance = (decimal)Math.Round(totalDistance / 1000, 1);
            routeModel.Ascent = (decimal)totalAscent;
            routeModel.Descent = (decimal)totalDescent;
        }

        protected virtual void CreateDirectory(string settingsDirectory)
        {
            Directory.CreateDirectory(settingsDirectory);
        }

        protected virtual bool DirectoryExists(string settingsDirectory)
        {
            return Directory.Exists(settingsDirectory);
        }

        protected virtual string[] GetFilesFromDirectory()
        {
            return Directory.GetFiles(_settings.Directory!, FileNamePattern, SearchOption.TopDirectoryOnly);
        }

        protected virtual Task WriteAllTextAsync(string path, string serialized)
        {
            return File.WriteAllTextAsync(path, serialized);
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
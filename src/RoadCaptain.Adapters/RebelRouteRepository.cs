// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class RebelRouteRepository : IRouteRepository
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly RouteStoreToDisk _routeStoreToDisk;
        private readonly ISegmentStore _segmentStore;
        private List<RouteModel>? _routeModels;

        public RebelRouteRepository(MonitoringEvents monitoringEvents, RouteStoreToDisk routeStoreToDisk, ISegmentStore segmentStore)
        {
            _monitoringEvents = monitoringEvents;
            _routeStoreToDisk = routeStoreToDisk;
            _segmentStore = segmentStore;
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

            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("Unable to determine application path and I can't load the Rebel Routes if I don't know where to start looking");
            }

            if (_routeModels == null)
            {
                _routeModels = await LoadRouteModels(path);
            }

            var query = _routeModels.AsQueryable();

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

            if (isLoop is not null)
            {
                query = query.Where(route => route.IsLoop == isLoop);
            }

            return query
                .ToArray()
                .Select(r =>
                {
                    r.IsReadOnly = IsReadOnly;
                    return r;
                })
                .ToArray();
        }

        private async Task<List<RouteModel>> LoadRouteModels(string path)
        {
            var routeFiles =
                Directory.GetFiles(Path.Combine(path, "Routes"), "RebelRoute-*.json", SearchOption.TopDirectoryOnly);

            var routeModels = new List<RouteModel>();

            foreach (var file in routeFiles)
            {
                try
                {
                    var serialized = await File.ReadAllTextAsync(file);
                    var plannedRoute = UpgradeIfNecessaryAndSerialize(serialized);

                    if (plannedRoute == null)
                    {
                        _monitoringEvents.Warning("Was unable to load the route from {File}", Path.GetFileName(file));
                        continue;
                    }

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

                    routeModel = CalculateMetrics(routeModel);

                    routeModels.Add(routeModel);
                }
                catch (Exception e)
                {
                    _monitoringEvents.Error(e, "Unable to deserialize route model from file '{File}'", file);
                }
            }

            return routeModels;
        }

        public Task<RouteModel> StoreAsync(PlannedRoute plannedRoute, List<Segment> segments)
        {
            throw new InvalidOperationException("Rebel route repository is read-only");
        }

        public string Name => "Zwift Insider - Rebel Routes";
        public bool IsReadOnly => true;
        public bool RequiresAuthentication => false;

        public Task DeleteAsync(Uri routeUri)
        {
            throw new InvalidOperationException("Rebel Routes are baked into RoadCaptain and can't be deleted.");
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

        private RouteModel CalculateMetrics(RouteModel routeModel)
        {
            if (routeModel.PlannedRoute == null)
            {
                return routeModel;
            }

            var distance = 0d;
            var ascent = 0d;
            var descent = 0d;

            var segments = _segmentStore.LoadSegments(routeModel.PlannedRoute.World!, routeModel.PlannedRoute.Sport);
            foreach (var seq in routeModel.PlannedRoute.RouteSegmentSequence)
            {
                var segment = segments.Single(s => s.Id == seq.SegmentId);

                distance += segment.Distance;

                if (seq.Direction == SegmentDirection.AtoB)
                {
                    ascent += segment.Ascent;
                    descent += segment.Descent;
                }
                else if(seq.Direction == SegmentDirection.BtoA)
                {
                    ascent += segment.Descent;
                    descent += segment.Ascent;
                }
            }

            routeModel.Distance = (decimal)Math.Round(distance / 1000, 1);
            routeModel.Ascent = (decimal)Math.Round(ascent, 1);
            routeModel.Descent = (decimal)Math.Round(descent, 1);

            return routeModel;
        }
    }
}

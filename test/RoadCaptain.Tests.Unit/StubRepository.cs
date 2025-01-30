// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    internal class StubRepository : IRouteRepository
    {
        private readonly bool _throwsOnSearch;
        private readonly int _numberOfRoutes;

        public StubRepository(string name = "TEST", bool throwsOnSearch = false, int numberOfRoutes = 1)
        {
            _throwsOnSearch = throwsOnSearch;
            _numberOfRoutes = numberOfRoutes;
            Name = name;
        }

        public Task<bool> IsAvailableAsync()
        {
            return Task.FromResult(true);
        }

        public async Task<RouteModel[]> SearchAsync(string? world = null, string? creator = null, string? name = null, string? zwiftRouteName = null,
            int? minDistance = null, int? maxDistance = null, int? minAscent = null, int? maxAscent = null,
            int? minDescent = null, int? maxDescent = null, bool? isLoop = null, string[]? komSegments = null,
            string[]? sprintSegments = null, CancellationToken cancellationToken = default)
        {
            // This only exists to ensure that this repository is properly asynchronous
            await Task.Delay(10);
            
            if (_throwsOnSearch)
            {
                throw new Exception("BANG!");
            }

            var routes = Enumerable
                .Range(1, _numberOfRoutes)
                .Select(number => new RouteModel { Name = "Route " + number })
                .ToArray();

            return routes;
        }

        public Task<RouteModel> StoreAsync(PlannedRoute plannedRoute, Uri? routeUri)
        {
            StoredRoutes.Add(plannedRoute);
            return Task.FromResult(new RouteModel());
        }

        public List<PlannedRoute> StoredRoutes { get; } = new();

        public string Name { get; }
        public bool IsReadOnly => false;
        public bool RequiresAuthentication => false;

        public Task DeleteAsync(Uri routeUri)
        {
            throw new NotImplementedException();
        }
    }
}

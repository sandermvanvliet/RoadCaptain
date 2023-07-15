using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    internal class StubRepository : IRouteRepository
    {
        public Task<bool> IsAvailableAsync()
        {
            return Task.FromResult(true);
        }

        public Task<RouteModel[]> SearchAsync(string? world = null, string? creator = null, string? name = null, string? zwiftRouteName = null,
            int? minDistance = null, int? maxDistance = null, int? minAscent = null, int? maxAscent = null,
            int? minDescent = null, int? maxDescent = null, bool? isLoop = null, string[]? komSegments = null,
            string[]? sprintSegments = null)
        {
            throw new NotImplementedException();
        }

        public Task<RouteModel> StoreAsync(PlannedRoute plannedRoute, string? token, List<Segment> segments)
        {
            StoredRoutes.Add(plannedRoute);
            return Task.FromResult(new RouteModel());
        }

        public List<PlannedRoute> StoredRoutes { get; } = new();

        public string Name => "TEST";
        public bool IsReadOnly => false;
    }
}
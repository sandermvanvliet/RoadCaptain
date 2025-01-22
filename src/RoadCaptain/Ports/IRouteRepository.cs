// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;

namespace RoadCaptain.Ports
{
    public interface IRouteRepository
    {
        Task<bool> IsAvailableAsync();

        Task<RouteModel[]> SearchAsync(string? world = null,
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
            string[]? sprintSegments = null);

        Task<RouteModel> StoreAsync(PlannedRoute plannedRoute, Uri? routeUri);

        string Name { get; }
        bool IsReadOnly { get; }
        bool RequiresAuthentication { get; }
        Task DeleteAsync(Uri routeUri);
    }
}
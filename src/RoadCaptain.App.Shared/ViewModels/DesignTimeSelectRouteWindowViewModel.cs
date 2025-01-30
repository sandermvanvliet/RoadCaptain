// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;

namespace RoadCaptain.App.Shared.ViewModels
{
    public class DesignTimeSelectRouteWindowViewModel : SelectRouteWindowViewModel
    {
        public DesignTimeSelectRouteWindowViewModel() : base(
            new SearchRoutesUseCase(new [] { new StubRouteRepository()}, new NopMonitoringEvents()),
            new RetrieveRepositoryNamesUseCase(new [] { new StubRouteRepository()}),
            new DesignTimeWindowService(),
            new StubWorldStore())
        {
            Repositories = new[]
            {
                "All",
                "Local"
            };

            Routes = new[]
            {
                new RouteViewModel(new RouteModel
                {
                    Ascent = 123,
                    Descent = 75,
                    Distance = 105,
                    CreatorName = "Joe Bloegs",
                    World = "watopia",
                    Id = 1,
                    IsLoop = false,
                    Name = "Design time route 1",
                    RepositoryName = "Local",
                    ZwiftRouteName = "ZRName1"
                }),
                
                new RouteViewModel(new RouteModel
                {
                    Ascent = 13,
                    Descent = 45,
                    Distance = 45,
                    CreatorName = "Joe Blogs",
                    World = "yorkshire",
                    Id = 2,
                    IsLoop = true,
                    Name = "Design time route 2",
                    RepositoryName = "Local",
                    ZwiftRouteName = "ZRName2"
                }),
                
                new RouteViewModel(new RouteModel
                {
                    Ascent = 13,
                    Descent = 45,
                    Distance = 45,
                    CreatorName = "Joe Blogs",
                    World = "makuri_islands",
                    Id = 3,
                    IsLoop = true,
                    Name = "Design time route 3",
                    RepositoryName = "Local",
                    ZwiftRouteName = "ZRName3"
                })
            };
        }
    }
    public class StubWorldStore : IWorldStore
    {
        public World[] LoadWorlds()
        {
            return new[]
            {
                new World { Id = "watopia", Name = "Watopia" },
                new World { Id = "makuri_islands", Name = "Makuri Islands" },
            };
        }

        public World? LoadWorldById(string id)
        {
            return null;
        }
    }

    internal class StubRouteRepository : IRouteRepository
    {
        public Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<RouteModel[]> SearchAsync(string? world = null, string? creator = null, string? name = null,
            string? zwiftRouteName = null,
            int? minDistance = null, int? maxDistance = null, int? minAscent = null, int? maxAscent = null,
            int? minDescent = null, int? maxDescent = null, bool? isLoop = null, string[]? komSegments = null,
            string[]? sprintSegments = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new[]
            {
                new RouteModel
                {
                    Name = "Demo route",
                    Ascent = 1500,
                    Descent = 1200,
                    Distance = 100,
                    CreatorName = "Sander van Vliet",
                    ZwiftRouteName = "Muir and the Mountain",
                    IsLoop = false,
                    Id = 1,
                    CreatorZwiftProfileId = "https://roadcaptain.nl"
                }
            });
        }

        public Task<RouteModel> StoreAsync(PlannedRoute plannedRoute, Uri? routeUri)
        {
            throw new System.NotImplementedException();
        }

        public string Name => "Local";
        public bool IsReadOnly => false;
        public bool RequiresAuthentication => false;

        public Task DeleteAsync(Uri routeUri)
        {
            throw new NotImplementedException();
        }
    }
}

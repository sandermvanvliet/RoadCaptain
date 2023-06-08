using System.Collections.Immutable;
using System.Threading.Tasks;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    internal class DesignTimeSaveRouteDialogViewModel : SaveRouteDialogViewModel
    {
        public DesignTimeSaveRouteDialogViewModel()
            : base(new DesignTimeWindowService(), new DummyUserPreferences(), new RouteViewModel(null, null), new RetrieveRepositoryNamesUseCase(new [] { new StubRouteRepository() }))
        {
            Repositories = new[] { "All", "Local" }.ToImmutableList();
        }
    }

    internal class StubRouteRepository : IRouteRepository
    {
        public Task<bool> IsAvailableAsync()
        {
            return Task.FromResult(true);
        }

        public Task<RouteModel[]> SearchAsync(string? world = null, string? creator = null, string? name = null,
            string? zwiftRouteName = null,
            int? minDistance = null, int? maxDistance = null, int? minAscent = null, int? maxAscent = null,
            int? minDescent = null, int? maxDescent = null, bool? isLoop = null, string[]? komSegments = null,
            string[]? sprintSegments = null)
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

        public Task<RouteModel> StoreAsync(PlannedRoute plannedRoute, OAuthToken token)
        {
            throw new System.NotImplementedException();
        }

        public string Name => "Local";
    }
}
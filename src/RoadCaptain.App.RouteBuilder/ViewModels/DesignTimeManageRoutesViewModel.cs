using System.Collections.Immutable;
using RoadCaptain.UseCases;

namespace RoadCaptain.App.RouteBuilder.ViewModels
{
    public class DesignTimeManageRoutesViewModel : ManageRoutesViewModel
    {
        public DesignTimeManageRoutesViewModel()
            : base(new RetrieveRepositoryNamesUseCase(new[] { new StubRouteRepository() }), null!, new DeleteRouteUseCase(new []{new StubRouteRepository()}))
        {
            Repositories = new[]
            {
                "All",
                "Local"
            }.ToImmutableList();

            Routes = new[]
            {
                new RoadCaptain.App.Shared.ViewModels.RouteViewModel(new RouteModel
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
                
                new RoadCaptain.App.Shared.ViewModels.RouteViewModel(new RouteModel
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
                
                new RoadCaptain.App.Shared.ViewModels.RouteViewModel(new RouteModel
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
            }.ToImmutableList();
        }
    }
}
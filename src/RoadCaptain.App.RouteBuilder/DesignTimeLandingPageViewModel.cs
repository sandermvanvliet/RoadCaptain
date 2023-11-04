using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.Ports;

namespace RoadCaptain.App.RouteBuilder
{
    public class DesignTimeLandingPageViewModel : LandingPageViewModel
    {
        public DesignTimeLandingPageViewModel() 
            : base(new DesignTimeWorldStore(), new DummyUserPreferences(), new DesignTimeWindowService())
        {
        }
    }

    public class DesignTimeWorldStore : IWorldStore
    {
        public World[] LoadWorlds()
        {
            return new[]
            {
                new World
                {
                    Id = "w1",
                    Name = "World 1",
                    Description = "World one"
                },
                new World
                {
                    Id = "w2",
                    Name = "World 2",
                    Description = "World two"
                }
            };
        }

        public World? LoadWorldById(string id)
        {
            return new World
            {
                Id = "id",
                Name = $"World {id}",
                Description = $"World {id}"
            };
        }
    }
}
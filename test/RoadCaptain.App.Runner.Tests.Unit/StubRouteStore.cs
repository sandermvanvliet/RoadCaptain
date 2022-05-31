using System;
using RoadCaptain.Ports;

namespace RoadCaptain.App.Runner.Tests.Unit
{
    public class StubRouteStore : IRouteStore
    {
        public PlannedRoute LoadFrom(string path)
        {
            if (path == "someroute.json")
            {
                return new PlannedRoute
                {
                    World = new World { Id = "watopia"},
                    Sport = SportType.Cycling
                };
            }

            return null;
        }

        public void Store(PlannedRoute route, string path)
        {
            throw new NotImplementedException();
        }
    }
}
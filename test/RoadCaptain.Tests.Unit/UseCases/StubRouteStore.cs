using System;
using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit.UseCases
{
    public class StubRouteStore : IRouteStore
    {
        public PlannedRoute LoadFrom(string path)
        {
            if (path == "someroute.json")
            {
                return new PlannedRoute();
            }

            return null;
        }

        public void Store(PlannedRoute route, string path)
        {
            throw new NotImplementedException();
        }
    }
}
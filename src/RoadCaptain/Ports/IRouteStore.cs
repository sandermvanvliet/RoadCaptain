namespace RoadCaptain.Ports
{
    public interface IRouteStore
    {
        PlannedRoute LoadFrom(string path);
        void Store(PlannedRoute route, string path);
    }
}

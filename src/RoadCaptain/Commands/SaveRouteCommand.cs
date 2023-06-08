namespace RoadCaptain.Commands
{
    public class SaveRouteCommand
    {
        public PlannedRoute Route { get; }
        public string RouteName { get; }
        public string RepositoryName { get; }

        public SaveRouteCommand(PlannedRoute route, string routeName, string repositoryName)
        {
            Route = route;
            RouteName = routeName;
            RepositoryName = repositoryName;
        }
    }
}
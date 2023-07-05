// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.Commands
{
    public class SaveRouteCommand
    {
        public PlannedRoute Route { get; }
        public string RouteName { get; }
        public string? RepositoryName { get; }
        public string? Token { get; }
        public string? OutputFilePath { get; }

        public SaveRouteCommand(
            PlannedRoute route, 
            string routeName,
            string? repositoryName, 
            string? token,
            string? outputFilePath)
        {
            Route = route;
            RouteName = routeName;
            RepositoryName = repositoryName;
            Token = token;
            OutputFilePath = outputFilePath;
        }
    }
}

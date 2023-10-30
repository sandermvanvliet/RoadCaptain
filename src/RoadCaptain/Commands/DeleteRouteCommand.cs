using System;

namespace RoadCaptain.Commands
{
    public record DeleteRouteCommand(Uri RouteUri, string RepositoryName);
}
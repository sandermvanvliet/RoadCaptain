using System;
using RoadCaptain.Commands;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class LoadRouteUseCase
    {
        private readonly IGameStateDispatcher _gameStateDispatcher;
        private readonly IRouteStore _routeStore;

        public LoadRouteUseCase(IGameStateDispatcher gameStateDispatcher, IRouteStore routeStore)
        {
            _gameStateDispatcher = gameStateDispatcher;
            _routeStore = routeStore;
        }

        public void Execute(LoadRouteCommand command)
        {
            if (string.IsNullOrEmpty(command.Path))
            {
                throw new ArgumentException("Route path must be a valid path", nameof(command));
            }

            var route = _routeStore.LoadFrom(command.Path);

            _gameStateDispatcher.RouteSelected(route);
        }
    }
}
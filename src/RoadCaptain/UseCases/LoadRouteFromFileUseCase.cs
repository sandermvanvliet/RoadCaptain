// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;
using RoadCaptain.Commands;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class LoadRouteFromFileUseCase
    {
        private readonly ConvertZwiftMapRouteUseCase _convertUseCase;
        private readonly IRouteStore _routeStore;

        public LoadRouteFromFileUseCase(ConvertZwiftMapRouteUseCase convertUseCase, IRouteStore routeStore)
        {
            _convertUseCase = convertUseCase;
            _routeStore = routeStore;
        }

        public PlannedRoute Execute(LoadFromFileCommand command)
        {
            if (string.IsNullOrEmpty(command.Path))
            {
                throw new ArgumentException("The path is empty and I can't load a route from nothing");
            }

            if (command.Path.EndsWith(".gpx", StringComparison.InvariantCultureIgnoreCase))
            {
                var convertedRoute = _convertUseCase.Execute(ZwiftMapRoute.FromGpxFile(command.Path));

                return convertedRoute;
            }
            
            return _routeStore.LoadFrom(command.Path);
        }
        
    }
}

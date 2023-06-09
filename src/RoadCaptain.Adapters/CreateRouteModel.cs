// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RoadCaptain.Adapters
{
    internal class CreateRouteModel
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        
        public CreateRouteModel(PlannedRoute plannedRoute)
        {
            if (plannedRoute.World == null || string.IsNullOrEmpty(plannedRoute.World.Id))
            {
                throw new ArgumentException("Planned route does not have a world set");
            }
            if (string.IsNullOrEmpty(plannedRoute.Name))
            {
                throw new ArgumentException("Planned route does not have a name set");
            }
            if (string.IsNullOrEmpty(plannedRoute.ZwiftRouteName))
            {
                throw new ArgumentException("Planned route does not have the Zwift route name set");
            }
            
            World = plannedRoute.World.Id;
            Name = plannedRoute.Name;
            ZwiftRouteName = plannedRoute.ZwiftRouteName;
            IsLoop = plannedRoute.IsLoop;
            Serialized = JsonConvert.SerializeObject(plannedRoute, SerializerSettings);
        }

        public string Serialized { get; }

        public bool IsLoop { get; }

        public string ZwiftRouteName { get; }

        public string Name { get; }

        public string World { get; }
    }
}

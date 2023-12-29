// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Text;
using RoadCaptain.App.Web.Commands;
using RoadCaptain.App.Web.Models;
using RoadCaptain.App.Web.Ports;

namespace RoadCaptain.App.Web.UseCases
{
    public class RecalculateHashes
    {
        private readonly IRouteStore _routeStore;
        private readonly IUserStore _userStore;

        public RecalculateHashes(IRouteStore routeStore, IUserStore userStore)
        {
            _routeStore = routeStore;
            _userStore = userStore;
        }

        public string Execute(RecalculateHashesCommand command)
        {
            var log = new StringBuilder();
            
            var routesToRecalculate = command.OnlyMissing
                ? _routeStore.GetRoutesWithoutHashes()
                : _routeStore.Search(null, null, null,null, null, null,null, null, null, null, null, null, null);

            foreach (var route in routesToRecalculate)
            {
                if (string.IsNullOrEmpty(route.CreatorName))
                {
                    log.AppendLine($"Route {route.Id} ({route.Name}) does not have a creator name");
                    continue;
                }
                
                var user = _userStore.GetByName(route.CreatorName);

                if (user == null)
                {
                    log.AppendLine($"Route {route.Id} ({route.Name}) has a creator name but I couldn't find the user");
                    continue;
                }
                
                var updateRouteModel = new UpdateRouteModel
                {
                    Ascent = route.Ascent,
                    Name = route.Name,
                    Descent = route.Descent,
                    Serialized = route.Serialized,
                    Distance = route.Distance,
                    IsLoop = route.IsLoop,
                    ZwiftRouteName = route.ZwiftRouteName
                };

                try
                {
                    var updated = _routeStore.Update(route.Id, updateRouteModel, user);
                
                    log.AppendLine($"Route {route.Id} ({route.Name}) now has hash {updated.Hash}");
                }
                catch (Exception e)
                {
                    log.AppendLine($"Route {route.Id} ({route.Name}) failed to update because {e.Message}");
                }
            }

            return log.ToString();
        }
    }
}

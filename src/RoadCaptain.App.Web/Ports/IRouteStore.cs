// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using RoadCaptain.App.Web.Adapters.EntityFramework;
using RoadCaptain.App.Web.Models;
using Route = RoadCaptain.App.Web.Adapters.EntityFramework.Route;

namespace RoadCaptain.App.Web.Ports
{
    public interface IRouteStore
    {
        Models.RouteModel[] Search(
            string? world, 
            string? creator,
            string? name,
            string? zwiftRouteName,
            decimal? minDistance,
            decimal? maxDistance,
            decimal? minAscent,
            decimal? maxAscent,
            decimal? minDescent,
            decimal? maxDescent,
            bool? isLoop,
            string[]? komSegments,
            string[]? sprintSegments);

        Models.RouteModel? GetById(long id);
        void Delete(long id);
        Models.RouteModel Update(long id, UpdateRouteModel updateModel, User currentUser);
        Models.RouteModel Store(CreateRouteModel createModel, User user);
        bool Exists(long id);
        Dictionary<string, long[]> FindDuplicates();
        Models.RouteModel[] GetRoutesWithoutHashes();
        Models.RouteModel[] GetAllRoutes();
    }
}


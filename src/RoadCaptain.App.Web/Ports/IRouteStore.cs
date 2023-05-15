// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using RoadCaptain.App.Web.Adapters.EntityFramework;
using RoadCaptain.App.Web.Models;

namespace RoadCaptain.App.Web.Ports
{
    public interface IRouteStore
    {
        RouteModel[] Search(
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

        RouteModel? GetById(long id);
        void Delete(long id);
        RouteModel Update(long id, UpdateRouteModel updateModel);
        RouteModel Store(CreateRouteModel createModel, User user);
    }
}


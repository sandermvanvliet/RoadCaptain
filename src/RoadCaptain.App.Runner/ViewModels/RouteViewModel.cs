// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain.App.Runner.ViewModels
{
    public class RouteViewModel
    {
        public RouteViewModel(RouteModel routeModel)
        {
            Id = routeModel.Id;
            CreatorName = routeModel.CreatorName;
            CreatorZwiftProfileId = routeModel.CreatorZwiftProfileId;
            Ascent = routeModel.Ascent;
            Descent = routeModel.Descent;
            Distance = routeModel.Distance;
            Name = routeModel.Name;
            IsLoop = routeModel.IsLoop;
            ZwiftRouteName = routeModel.ZwiftRouteName;
            Serialized = routeModel.Serialized;
            RepositoryName = routeModel.RepositoryName ?? "(unknown)";
            Uri = routeModel.Uri;
            PlannedRoute = routeModel.PlannedRoute;
        }

        public long Id { get; set; }
        public string? CreatorName { get; set; }
        public string? CreatorZwiftProfileId { get; set; }
        public string? Name { get; set; }
        public string? ZwiftRouteName { get; set; }
        public decimal Distance { get; set; }
        public decimal Ascent { get; set; }
        public decimal Descent { get; set; }
        public bool IsLoop { get; }
        public string? Serialized { get; }
        public string RepositoryName { get; }
        public Uri Uri { get; set; }
        public PlannedRoute? PlannedRoute { get; set; }

        public RouteModel? AsRouteModel()
        {
            return new RouteModel
            {
                Id = Id,
                CreatorName = CreatorName,
                CreatorZwiftProfileId = CreatorZwiftProfileId,
                Ascent = Ascent,
                Descent = Descent,
                Distance = Distance,
                Name = Name,
                IsLoop = IsLoop,
                ZwiftRouteName = ZwiftRouteName,
                Serialized = Serialized,
                RepositoryName = RepositoryName,
                Uri = Uri,
                PlannedRoute = PlannedRoute
            };
        }
    }
}

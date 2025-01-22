// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.App.Shared.ViewModels
{
    public class DesignTimeRoutesListViewModel
    {
        public static RouteViewModel[] Instance { get; } = new[]
        {
            new RouteViewModel(new RouteModel
            {
                Ascent = 123,
                Descent = 75,
                Distance = 105,
                CreatorName = "Joe Bloegs",
                World = "watopia",
                Id = 1,
                IsLoop = false,
                Name = "Design time route 1",
                RepositoryName = "Local",
                ZwiftRouteName = "ZRName1"
            }),

            new RouteViewModel(new RouteModel
            {
                Ascent = 13,
                Descent = 45,
                Distance = 45,
                CreatorName = "Joe Blogs",
                World = "yorkshire",
                Id = 2,
                IsLoop = true,
                Name = "Design time route 2",
                RepositoryName = "Local",
                ZwiftRouteName = "ZRName2"
            }),

            new RouteViewModel(new RouteModel
            {
                Ascent = 13,
                Descent = 45,
                Distance = 45,
                CreatorName = "Joe Blogs",
                World = "makuri_islands",
                Id = 3,
                IsLoop = true,
                Name = "Design time route 3",
                RepositoryName = "Local",
                ZwiftRouteName = "ZRName3"
            })
        };
    }
}

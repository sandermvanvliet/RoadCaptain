// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.Ports;

namespace RoadCaptain.App.RouteBuilder
{
    public class DesignTimeLandingPageViewModel : LandingPageViewModel
    {
        public DesignTimeLandingPageViewModel() 
            : base(new DesignTimeWorldStore(), new DummyUserPreferences(), new DesignTimeWindowService(), null!, null!, null!)
        {
            InProgress = false;
        }
    }

    public class DesignTimeWorldStore : IWorldStore
    {
        public World[] LoadWorlds()
        {
            return new[]
            {
                new World
                {
                    Id = "w1",
                    Name = "World 1",
                    Description = "World one"
                },
                new World
                {
                    Id = "w2",
                    Name = "World 2",
                    Description = "World two"
                }
            };
        }

        public World? LoadWorldById(string id)
        {
            return new World
            {
                Id = "id",
                Name = $"World {id}",
                Description = $"World {id}"
            };
        }
    }
}

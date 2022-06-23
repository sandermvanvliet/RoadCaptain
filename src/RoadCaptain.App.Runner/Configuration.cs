// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Microsoft.Extensions.Configuration;

namespace RoadCaptain.App.Runner
{
    public class Configuration
    {
        public Configuration(IConfiguration configuration)
        {
            configuration?.GetSection("Zwift").Bind(this);
        }

        public string? Route { get; set; }
    }
}

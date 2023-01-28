// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Microsoft.Extensions.Configuration;

namespace RoadCaptain.Runner
{
    public class Configuration
    {
        public Configuration(IConfiguration configuration)
        {
            configuration?.GetSection("Zwift").Bind(this);
        }

        public string Route { get; set; }
        public string AccessToken { get; set; }
    }
}

// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Microsoft.Extensions.Configuration;

namespace RoadCaptain.Adapters
{
    internal class HttpRouteRepositorySettings
    {
        public HttpRouteRepositorySettings(IConfiguration configuration)
        {
            configuration.Bind(this);
            IsValid = Uri.ToString() != "https://roadcaptain.nl";
        }

        public bool IsValid { get; }

        public string Name { get; init; } = "(unknown)";

        public Uri Uri { get; init; } = new("https://roadcaptain.nl");
    }
}

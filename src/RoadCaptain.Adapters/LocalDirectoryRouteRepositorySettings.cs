using Microsoft.Extensions.Configuration;

namespace RoadCaptain.Adapters
{
    internal class LocalDirectoryRouteRepositorySettings
    {
        public LocalDirectoryRouteRepositorySettings(IConfiguration configuration)
        {
            configuration.Bind(this);
            IsValid = !string.IsNullOrEmpty(Directory);
        }

        public bool IsValid { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty, property is assigned through Bind() in the constructor
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Name { get; init; } = "(unknown)";

        // ReSharper disable once UnassignedGetOnlyAutoProperty, property is assigned through Bind() in the constructor
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? Directory { get; init; }
    }
}
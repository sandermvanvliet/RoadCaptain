using Microsoft.Extensions.Configuration;

namespace RoadCaptain.Runner
{
    public class Configuration
    {
        public Configuration(IConfiguration configuration)
        {
            configuration.GetSection("Zwift").Bind(this);
        }

        public string Route { get; set; }
        public string AccessToken { get; set; }
    }
}
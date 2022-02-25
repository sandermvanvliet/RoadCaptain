using Microsoft.Extensions.Configuration;

namespace RoadCaptain.Host.Console
{
    internal class Configuration
    {
        public Configuration(IConfiguration configuration)
        {
            configuration.GetSection("Zwift").Bind(this);
        }

        public string Username { get; set; }
        public string Password { get; set; }
    }
}
using Microsoft.Extensions.Configuration;

namespace RoadCaptain.Host.Console
{
    internal class Configuration
    {
        public Configuration(IConfiguration configuration)
        {
            configuration.Bind(this);
        }

        public string Username { get; set; }
        public string Password { get; set; }
    }
}
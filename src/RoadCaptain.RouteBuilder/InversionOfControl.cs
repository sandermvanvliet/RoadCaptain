using System.Windows.Threading;
using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace RoadCaptain.RouteBuilder
{
    public class InversionOfControl
    {
        public static ContainerBuilder ConfigureContainer(IConfigurationRoot configuration, ILogger logger, Dispatcher dispatcher)
        {
            var builder = new ContainerBuilder();

            builder.Register(_ => logger).SingleInstance();
            builder.Register<IConfiguration>(_ => configuration).SingleInstance();
            
            // Wire up registrations through the autofac.json file
            builder.RegisterModule(new ConfigurationModule(configuration));

            // Register dispatcher here because MainModule does not know of it
            builder.RegisterInstance(dispatcher).AsSelf().SingleInstance();

            return builder;
        }
    }
}
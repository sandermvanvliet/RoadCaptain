using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;

namespace RoadCaptain.Host.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = CreateLogger();
            
            try
            {
                var host = CreateHostBuilder(args, logger).Build();
                
                RegisterLifetimeEvents(host);

                host.Run();
            }
            finally
            {
                // Flush the logger
                logger.Dispose();
            }
        }

        private static Logger CreateLogger()
        {
            return new LoggerConfiguration()
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        // The logger instance is passed in because it needs to go into the IoC container and is used directly by UseSerilog
        private static IHostBuilder CreateHostBuilder(string[] args, ILogger logger) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((_, builder) =>
                {
                    builder.Register(_ => logger).SingleInstance();

                    var configurationRoot = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", true)
                        .Build();

                    var configuration = new Configuration();
                    configurationRoot.Bind(configuration);
                    builder.Register(_ => configuration).SingleInstance();

                    builder.RegisterAssemblyModules(typeof(Program).Assembly);
                    builder.RegisterAssemblyModules(typeof(MonitoringEvents).Assembly);

                    // TODO: find a nicer way to do this
                    builder.RegisterAssemblyModules(typeof(RoadCaptain.Adapters.AdaptersModule).Assembly);
                })
                .UseSerilog(logger);

        private static void RegisterLifetimeEvents(IHost host)
        {
            var monitoringEvents = host.Services.GetService(typeof(MonitoringEvents)) as MonitoringEvents;
            var lifetime = (IHostApplicationLifetime)host.Services.GetService(typeof(IHostApplicationLifetime));
            if (lifetime == null)
            {
                return;
            }

            lifetime.ApplicationStarted.Register(() => monitoringEvents.ApplicationStarted());
            lifetime.ApplicationStopping.Register(() => monitoringEvents.ApplicationStopping());
            lifetime.ApplicationStopped.Register(() => monitoringEvents.ApplicationStopped());
        }
    }
}

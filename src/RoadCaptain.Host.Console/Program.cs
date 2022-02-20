using System;
using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;

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
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.File($"roadcaptain-log-{DateTime.UtcNow:yyyy-MM-ddTHHmmss}.log", restrictedToMinimumLevel: LogEventLevel.Debug)
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        // The logger instance is passed in because it needs to go into the IoC container and is used directly by UseSerilog
        private static IHostBuilder CreateHostBuilder(string[] args, ILogger logger) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(builder =>
                {
                    builder
                        .AddJsonFile("appsettings.json", true)
                        .AddJsonFile("autofac.json")
                        .AddJsonFile("autofac.development.json", true);
                })
                .ConfigureContainer<ContainerBuilder>((_, builder) =>
                {
                    builder.Register(_ => logger).SingleInstance();

                    var configuration = new Configuration();
                    _.Configuration.Bind(configuration);
                    builder.Register(_ => configuration).SingleInstance();

                    builder.RegisterAssemblyModules(typeof(Program).Assembly);

                    // Wire up registrations through the autofac.json file
                    builder.RegisterModule(new ConfigurationModule(_.Configuration));
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
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

using System;
using System.Diagnostics;
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

            IHost host;

            try
            {
                host = CreateHost(args, logger);

                RegisterLifetimeEvents(host);
            }
            catch (Exception ex)
            {
                Debugger.Break();

                logger.Error(ex, "Failed to configure host");
                Environment.ExitCode = 1;
                
                // Flush the logger, note that this doesn't happen
                // in the finally because we need the logger in the
                // run phase if no exception occurs.
                logger.Dispose();

                return;
            }

            try
            {
                host.Run();
            }
            catch (Exception ex)
            {
                Debugger.Break();

                logger.Error(ex, "Unhandled exception caught");
                Environment.ExitCode = 1;
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
                .WriteTo.Console(LogEventLevel.Information)
                .WriteTo.File($"roadcaptain-log-{DateTime.UtcNow:yyyy-MM-ddTHHmmss}.log", LogEventLevel.Debug)
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        // The logger instance is passed in because it needs to go into the IoC container
        // and is used directly by UseSerilog()
        private static IHost CreateHost(string[] args, ILogger logger)
        {
            return Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder(args)
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

                    // Wire up registrations through the autofac.json file
                    builder.RegisterModule(new ConfigurationModule(_.Configuration));
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseSerilog(logger)
                .Build();
        }

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

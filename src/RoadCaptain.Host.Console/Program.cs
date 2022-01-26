using Autofac;
using Autofac.Extensions.DependencyInjection;
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
            
            var builder = new ContainerBuilder();
            
            // ReSharper disable once AccessToDisposedClosure
            builder.Register<ILogger>(_ => logger).SingleInstance();
            builder.RegisterType<MonitoringEvents>().As<MonitoringEventsWithSerilog>().SingleInstance();
            builder.RegisterType<RoadCaptainConsoleHost>().As<IHostedService>();

            builder.RegisterModule<DomainModule>();

            try
            {
                CreateHostBuilder(args, builder, logger).Build().Run();
            }
            finally
            {
                // Flush the logger
                logger.Dispose();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args, ContainerBuilder containerBuilder, ILogger logger) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    containerBuilder.Populate(services);
                })
                .UseSerilog(logger);

        private static Logger CreateLogger()
        {
            return new LoggerConfiguration()
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .CreateLogger();
        }
    }
}

using System;
using System.Windows.Forms;
using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace RoadCaptain.Monitor
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var logger = CreateLogger();
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(_ => logger).SingleInstance();

            var configurationRoot = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("autofac.json")
                .AddJsonFile("autofac.development.json", true)
                .Build();

            var configuration = new Configuration();
            configurationRoot.Bind(configuration);
            builder.Register(_ => configuration).SingleInstance();

            builder.RegisterAssemblyModules(typeof(Program).Assembly);

            var configurationModule = new ConfigurationModule(configurationRoot);
            builder.RegisterModule(configurationModule);
            
            builder
                .RegisterAssemblyTypes(typeof(Program).Assembly)
                .Except<Configuration>();

            builder.RegisterType<MonitoringEventsWithSerilog>().As<MonitoringEvents>().SingleInstance();
            
            var container = builder.Build();

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(container.Resolve<MainWindow>());
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
    }
}

using System;
using System.Windows;
using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace RoadCaptain.Runner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class App : Application
    {
        private readonly IContainer _container;
        private readonly Logger _logger;

        public App()
        {
            _logger = CreateLogger();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("autofac.json")
                .AddJsonFile("autofac.development.json", true)
                .Build();

            var builder = new ContainerBuilder();
            
            builder.Register(_ => _logger).SingleInstance();
            builder.Register(_ => configuration).SingleInstance();
            
            builder.RegisterType<Configuration>().AsSelf().SingleInstance();

            // Wire up registrations through the autofac.json file
            builder.RegisterModule(new ConfigurationModule(configuration));

            _container = builder.Build();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = _container.Resolve<MainWindow>();

            mainWindow.Show();
        }

        private static Logger CreateLogger()
        {
            return new LoggerConfiguration()
                .WriteTo.Console(LogEventLevel.Information)
                .WriteTo.File($"roadcaptain-log-{DateTime.UtcNow:yyyy-MM-ddTHHmmss}.log", LogEventLevel.Debug)
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            // Flush the logger
            _logger?.Dispose();
        }
    }
}

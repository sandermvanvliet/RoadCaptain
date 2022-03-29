// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Windows;
using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

namespace RoadCaptain.Runner
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class App : Application
    {
        private readonly IContainer _container;
        private readonly Logger _logger;
        private readonly Engine _engine;
        private readonly MonitoringEvents _monitoringEvents;

        public App()
        {
            _logger = LoggerBootstrapper.CreateLogger();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("autofac.runner.json")
                .AddJsonFile("autofac.runner.development.json", true)
                .Build();

            var builder = new ContainerBuilder();

            builder.Register<ILogger>(_ => _logger).SingleInstance();
            builder.Register<IConfiguration>(_ => configuration).SingleInstance();

            builder.RegisterType<Configuration>().AsSelf().SingleInstance();

            builder.Register(_ => AppSettings.Default).SingleInstance();

            // Wire up registrations through the autofac.json file
            builder.RegisterModule(new ConfigurationModule(configuration));

            _container = builder.Build();

            _engine = _container.Resolve<Engine>();
            _monitoringEvents = _container.Resolve<MonitoringEvents>();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _monitoringEvents.ApplicationStarted();

            _engine.Start();

            var mainWindow = _container.Resolve<MainWindow>();

            mainWindow.Show();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            _monitoringEvents.ApplicationStopping();

            _engine.Stop();

            _monitoringEvents.ApplicationStopped();

            // Flush the logger
            _logger?.Dispose();
        }
    }
}
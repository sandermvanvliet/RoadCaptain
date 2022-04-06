// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Windows;
using Autofac;
using Microsoft.Extensions.Configuration;
using Serilog.Core;

namespace RoadCaptain.Runner
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class App : Application
    {
        private readonly Logger _logger;
        private readonly Engine _engine;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IWindowService _windowService;

        public App()
        {
            _logger = LoggerBootstrapper.CreateLogger();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("autofac.runner.json")
                .AddJsonFile("autofac.runner.development.json", true)
                .Build();

            var container = InversionOfControl
                .ConfigureContainer(configuration, _logger, Dispatcher)
                .Build();

            _engine = container.Resolve<Engine>();
            _monitoringEvents = container.Resolve<MonitoringEvents>();
            _windowService = container.Resolve<IWindowService>();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _monitoringEvents.ApplicationStarted();

            _windowService.ShowMainWindow();

            _engine.Start();
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
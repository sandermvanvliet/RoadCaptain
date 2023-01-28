// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Diagnostics;
using System.Linq;
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
            
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                _logger.Fatal(args.ExceptionObject as Exception, "Unhandled exception occurred");
            };

            DispatcherUnhandledException += (sender, args) =>
            {
                _logger.Error(args.Exception, "Unhandled exception in dispatcher");
            };

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

            if (IsRoadCaptainRunning())
            {
                _windowService.ShowAlreadyRunningDialog();
                _monitoringEvents.Warning("Another instance of RoadCaptain is already running");
                Shutdown(-1);
                return;
            }

            _windowService.ShowMainWindow();

            _engine.Start();
        }

        private static bool IsRoadCaptainRunning()
        {
            var processName = Process.GetCurrentProcess().ProcessName;

            return Process.GetProcesses().Count(p => p.ProcessName == processName) > 1;
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
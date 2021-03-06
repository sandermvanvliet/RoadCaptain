// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Windows;
using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Core;

namespace RoadCaptain.RouteBuilder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class App : Application
    {
        private readonly Logger _logger;
        private readonly IWindowService _windowService;

        public App()
        {
            _logger = CreateLogger();
            
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
                .AddJsonFile("autofac.routebuilder.json")
                .AddJsonFile("autofac.routebuilder.development.json", true)
                .Build();

            var container = InversionOfControl
                .ConfigureContainer(configuration, _logger, Dispatcher)
                .Build();

            _windowService = container.Resolve<IWindowService>();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _windowService.ShowMainWindow();
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


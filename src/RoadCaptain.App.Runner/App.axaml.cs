// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using RoadCaptain.App.Runner.Views;
using Serilog.Core;

namespace RoadCaptain.App.Runner
{
    public partial class App : Application
    {
        private readonly Logger _logger;
        private readonly IWindowService _windowService;
        private readonly Engine _engine;
        private readonly MonitoringEvents _monitoringEvents;

        public App()
        {
            _logger = Program.Logger;

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                _logger.Fatal(args.ExceptionObject as Exception, "Unhandled exception occurred");
            };

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("autofac.app.runner.json")
                .AddJsonFile("autofac.app.runner.development.json", true)
                .Build();

            var container = InversionOfControl
                .ConfigureContainer(configuration, _logger, Dispatcher.UIThread)
                .Build();

            _engine = container.Resolve<Engine>();
            _monitoringEvents = container.Resolve<MonitoringEvents>();
            _windowService = container.Resolve<IWindowService>();
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow == null)
            {
                _windowService.SetLifetime(desktop);
                desktop.Startup += App_OnStartup;
                desktop.Exit += App_OnExit;

                if (Design.IsDesignMode)
                {
                    desktop.MainWindow = new MainWindow();
                }
                else
                {
                    _windowService.ShowMainWindow();
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        protected override void LogBindingError(AvaloniaProperty property, Exception e)
        {
            _logger.Error(e, "Binding error on {PropertyName}", property.Name);
        }

        private void App_OnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            _monitoringEvents.ApplicationStarted();

            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (IsRoadCaptainRunning())
                {
                    await _windowService.ShowAlreadyRunningDialog();

                    _monitoringEvents.Warning("Another instance of RoadCaptain is already running");

                    _windowService.Shutdown(-1);
                }
            });

            _engine.Start();
        }

        private void App_OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            _monitoringEvents.ApplicationStopping();

            _engine.Stop();

            _monitoringEvents.ApplicationStopped();

            // Flush the logger
            _logger.Dispose();
        }

        private static bool IsRoadCaptainRunning()
        {
            var processName = Process.GetCurrentProcess().ProcessName;

            var processes = Process.GetProcesses();
            return processes.Count(p => p.ProcessName == processName) > 1;
        }
    }
}

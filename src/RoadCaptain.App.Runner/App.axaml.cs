// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using RoadCaptain.App.Runner.Views;
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.Dialogs;
using Serilog;
using Serilog.Core;

namespace RoadCaptain.App.Runner
{
    public partial class App : Application
    {
        private readonly Logger _logger;
        private readonly IWindowService _windowService;
        private readonly Engine _engine;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IUserPreferences _userPreferences;

        public App()
        {
            // When the Avalonia designer is used the Program class isn't called and the logger is null.
            // To make sure nothing else blows up we need to initialize it with a default logger.
            _logger = Program.Logger ?? new LoggerConfiguration().WriteTo.Debug().CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                _logger.Fatal(args.ExceptionObject as Exception, "Unhandled exception occurred");
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                _logger.Fatal(args.Exception, "Unhandled exception occurred in task");
            };

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.routerepositories.json", true)
                .AddJsonFile(Path.Combine(PlatformPaths.GetUserDataDirectory(), "routerepositories.json"), true)
                .Build();

            var container = InversionOfControl
                .ConfigureContainer(configuration, _logger, Dispatcher.UIThread)
                .Build();

            // Ensure user preferences are loaded
            _userPreferences = container.Resolve<IUserPreferences>();
            _userPreferences.Load();

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

        private void App_OnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            _monitoringEvents.ApplicationStarted();

            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (IsRoadCaptainRunning())
                {
                    await _windowService.ShowAlreadyRunningDialog("RoadCaptain Runner");

                    _monitoringEvents.Warning("Another instance of RoadCaptain is already running");

                    _windowService.Shutdown(-1);
                }
            });

            _engine.Start();
        }

        private void App_OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            _monitoringEvents.ApplicationStopping();

            _userPreferences.Save();

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

        private async void AboutRoadCaptainMenuItem_OnClick(object? sender, EventArgs e)
        {
            var dialog = new AboutRoadCaptainDialog
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
                    
            if (Application.Current is
                { ApplicationLifetime: IClassicDesktopStyleApplicationLifetime { MainWindow: { } mainWindow } })
            {
                await dialog.ShowDialog(mainWindow);
            }
        }
    }
}

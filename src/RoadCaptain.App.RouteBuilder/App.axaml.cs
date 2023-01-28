// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using RoadCaptain.App.RouteBuilder.Views;
using RoadCaptain.App.Shared.Dialogs;
using Serilog;
using Serilog.Core;
using RoadCaptain.App.Shared.Commands;
using System.Runtime.InteropServices;
using System.Security.Policy;

namespace RoadCaptain.App.RouteBuilder
{
    public partial class App : Application
    {
        private readonly Logger _logger;
        private readonly IContainer _container;
        private readonly IUserPreferences _userPreferences;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IWindowService _windowService;

        public App()
        {
            // When the Avalonia designer is used the Program class isn't called and the logger is null.
            // To make sure nothing else blows up we need to initialize it with a default logger.
            _logger = Program.Logger ?? new LoggerConfiguration().WriteTo.Debug().CreateLogger();
            
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                _logger.Fatal(args.ExceptionObject as Exception, "Unhandled exception occurred");
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                _logger.Fatal(args.Exception, "Unhandled exception occurred in task");
            };
            
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .Build();

            _container = InversionOfControl
                .ConfigureContainer(configuration, _logger, Dispatcher.UIThread)
                .Build();

            // Ensure user preferences are loaded
            _userPreferences = _container.Resolve<IUserPreferences>();
            _userPreferences.Load();
            
            _monitoringEvents = _container.Resolve<MonitoringEvents>();
            _windowService = _container.Resolve<IWindowService>();
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
                    _container
                        .Resolve<IWindowService>()
                        .ShowMainWindow(ApplicationLifetime);
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
                if (IsRouteBuilderRunning())
                {
                    await _windowService.ShowAlreadyRunningDialog();

                    _monitoringEvents.Warning("Another instance of RoadCaptain is already running");

                    _windowService.Shutdown(-1);
                }
            });
        }

        private void App_OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            _monitoringEvents.ApplicationStopping();

            _userPreferences.Save();

            _monitoringEvents.ApplicationStopped();

            // Flush the logger
            _logger.Dispose();
        }

        private static bool IsRouteBuilderRunning()
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

        private void Documentation_OnClick(object? sender, EventArgs e)
        {
            var url = "https://roadcaptain.nl";

            if (Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                // Code from Avalonia: AboutAvaloniaDialog.cs
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? url : "open",
                    Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? url : "",
                    CreateNoWindow = true,
                    UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                });
            }
        }
    }
}


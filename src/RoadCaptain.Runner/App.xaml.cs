// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
        private const string CompanyName = "Codenizer BV";
        private const string ApplicationName = "RoadCaptain";
        private readonly IContainer _container;
        private readonly Logger _logger;
        private List<IHostedService> _hostedServices = new();
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly List<Task> _hostedServiceTasks = new();

        public App()
        {
            _logger = CreateLogger();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("autofac.runner.json")
                .AddJsonFile("autofac.runner.development.json", true)
                .Build();

            var builder = new ContainerBuilder();
            
            builder.Register<ILogger>(_ => _logger).SingleInstance();
            builder.Register<IConfiguration>(_ => configuration).SingleInstance();
            
            builder.RegisterType<Configuration>().AsSelf().SingleInstance();

            builder.Register<AppSettings>(_ => AppSettings.Default).SingleInstance();

            // Wire up registrations through the autofac.json file
            builder.RegisterModule(new ConfigurationModule(configuration));

            _container = builder.Build();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _hostedServices = _container.Resolve<IEnumerable<IHostedService>>().ToList();

            foreach (var service in _hostedServices)
            {
                _hostedServiceTasks.Add(Task.Factory.StartNew(() => service.StartAsync(_tokenSource.Token)));
            }

            var mainWindow = _container.Resolve<MainWindow>();

            mainWindow.Show();
        }

        private static Logger CreateLogger()
        {
            var loggerConfiguration = new LoggerConfiguration().Enrich.FromLogContext();
            var logFileName = $"roadcaptain-log-{DateTime.UtcNow:yyyy-MM-ddTHHmmss}.log";
            
#if DEBUG
            // In debug builds always write to the current directory for simplicity sake
            // as that makes the log file easier to pick up from bin\Debug
            var logFilePath = logFileName;
#else
            // Because we install into Program Filex (x86) we can't write a log file
            // there when running as a regular user. Good Windows citizenship also
            // means we should write data to the right place which is in the user
            // AppData folder.
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            
            CreateDirectoryIfNotExists(Path.Combine(localAppDataFolder, CompanyName));
            CreateDirectoryIfNotExists(Path.Combine(localAppDataFolder, CompanyName, ApplicationName));
            
            var logFilePath = Path.Combine(
                            localAppDataFolder,
                            CompanyName, 
                            ApplicationName, 
                            logFileName);
#endif

            return loggerConfiguration
                .WriteTo.File(logFilePath, LogEventLevel.Debug)
                .CreateLogger();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            foreach (var service in _hostedServices)
            {
                service.StopAsync(_tokenSource.Token)
                    .GetAwaiter()
                    .GetResult();
            }

            _tokenSource.Cancel();

            foreach (var task in _hostedServiceTasks)
            {
                try
                {
                    task.GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    // Nop, this is what we expect
                }
            }

            // Flush the logger
            _logger?.Dispose();
        }

        private static void CreateDirectoryIfNotExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}


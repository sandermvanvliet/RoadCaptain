using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                .AddJsonFile("autofac.json")
                .AddJsonFile("autofac.development.json", true)
                .Build();

            var builder = new ContainerBuilder();
            
            builder.Register<ILogger>(_ => _logger).SingleInstance();
            builder.Register<IConfiguration>(_ => configuration).SingleInstance();
            
            builder.RegisterType<Configuration>().AsSelf().SingleInstance();

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
            return new LoggerConfiguration()
                .WriteTo.Console(LogEventLevel.Information)
                .WriteTo.File($"roadcaptain-log-{DateTime.UtcNow:yyyy-MM-ddTHHmmss}.log", LogEventLevel.Debug)
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
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
    }
}

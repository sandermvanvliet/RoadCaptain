using System;
using Autofac;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Runner.Views;
using Serilog.Core;

namespace RoadCaptain.App.Runner
{
    public partial class App : Application
    {
        private readonly Logger _logger;
        private readonly IWindowService _windowService;

        public App()
        {
            _logger = LoggerBootstrapper.CreateLogger();
            
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
                if (Design.IsDesignMode)
                {
                    desktop.MainWindow = new MainWindow();
                }
                else
                {
                    _windowService.ShowMainWindow(ApplicationLifetime);
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        protected override void LogBindingError(AvaloniaProperty property, Exception e)
        {
            _logger.Error(e, "Binding error on {PropertyName}", property.Name);
        }
    }
}

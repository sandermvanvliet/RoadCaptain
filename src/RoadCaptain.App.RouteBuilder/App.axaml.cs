using System;
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

namespace RoadCaptain.App.RouteBuilder
{
    public partial class App : Application
    {
        private readonly Logger _logger;
        private readonly IContainer _container;

        public App()
        {
            // When the Avalonia designer is used the Program class isn't called and the logger is null.
            // To make sure nothing else blows up we need to initialize it with a default logger.
            _logger = Program.Logger ?? new LoggerConfiguration().WriteTo.Debug().CreateLogger();
            
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                _logger.Fatal(args.ExceptionObject as Exception, "Unhandled exception occurred");
            };
            
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .Build();

            _container = InversionOfControl
                .ConfigureContainer(configuration, _logger, Dispatcher.UIThread)
                .Build();
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

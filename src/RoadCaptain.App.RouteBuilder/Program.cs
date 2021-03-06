using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Serilog.Core;
#if MACOS
using System.IO;
#endif

namespace RoadCaptain.App.RouteBuilder
{
    internal class Program
    {
        internal static Logger? Logger;
        
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            #if MACOS
            // When launching from an app bundle (.app) the working directory
            // is set to be / which prevents us from loading resources...
            if(Environment.CurrentDirectory == "/")
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            }
            #endif
            
            Logger = LoggerBootstrapper.CreateLogger();

            try
            {
                Logger.Information("Starting RouteBuilder");

                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);

                Logger.Information("RouteBuilder exiting");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Something went really wrong!");
            }
            finally
            {
                Logger.Dispose();
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}

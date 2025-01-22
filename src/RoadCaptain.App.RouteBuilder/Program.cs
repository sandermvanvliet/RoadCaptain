// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Avalonia;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Logging;
using Logger = Serilog.Core.Logger;

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
            if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // When launching from an app bundle (.app) the working directory
                // is set to be / which prevents us from loading resources...
                if(Environment.CurrentDirectory == "/")
                {
                    var assemblyLocation = typeof(Program).Assembly.Location;
                    var currentDirectory = Path.GetDirectoryName(assemblyLocation);

                    if (!string.IsNullOrEmpty(currentDirectory))
                    {
                        Environment.CurrentDirectory = currentDirectory;
                    }
                    else
                    {
                        throw new Exception(
                            "Unable to determine application startup directory, can't continue because I can't load my resources");
                    }
                }
            }
            
            Logger = LoggerBootstrapper.CreateLogger();

            try
            {
                Logger.Information("Starting RouteBuilder");

                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);

                Logger.Information("RouteBuilder exiting");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Something went really wrong!");
                Environment.ExitCode = 1;
            }
            finally
            {
                Logger.Dispose();
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        // ReSharper disable once MemberCanBePrivate.Global
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace(LogEventLevel.Information);
    }
}


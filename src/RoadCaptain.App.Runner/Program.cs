// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Avalonia;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Logging;
using Logger = Serilog.Core.Logger;

namespace RoadCaptain.App.Runner
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
                    var currentDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                    
                    if (string.IsNullOrEmpty(currentDirectory))
                    {
                        throw new Exception("Unable to determine application startup directory");
                    }

                    Environment.CurrentDirectory = currentDirectory;
                }
            }

            Logger = LoggerBootstrapper.CreateLogger();

            try
            {
                Logger.Information("Starting Runner");

                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);

                Logger.Information("Runner exiting");
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                {
                    Debugger.Break();
                }

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
                .LogToTrace(LogEventLevel.Information);
    }
}


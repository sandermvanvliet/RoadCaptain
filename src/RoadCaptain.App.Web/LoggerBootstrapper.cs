// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Diagnostics;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace RoadCaptain.App.Web
{
    public class LoggerBootstrapper
    {
        public static Logger CreateLogger()
        {
            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug();

            loggerConfiguration = loggerConfiguration
                .MinimumLevel.Information();

            if (Debugger.IsAttached)
            {
                loggerConfiguration = loggerConfiguration
                    .WriteTo.Debug(LogEventLevel.Debug);
            }
            else if("1".Equals(Environment.GetEnvironmentVariable("RUN_IN_SYSTEMD")))
            {
                loggerConfiguration = loggerConfiguration
                    .WriteTo.Console();
            }
            else
            {
                // In debug builds always write to the current directory for simplicity sake
                // as that makes the log file easier to pick up from bin\Debug
                var logFilePath = $"roadcaptain-web-log-{DateTime.UtcNow:yyyy-MM-ddTHHmmss}.log";

                logFilePath = CreateLoggerForReleaseMode(logFilePath);
                
                loggerConfiguration = loggerConfiguration
                    .WriteTo.File(logFilePath, LogEventLevel.Debug);                
            }

            return loggerConfiguration
                .CreateLogger();
        }

        // ReSharper disable once UnusedMember.Local
        private static string CreateLoggerForReleaseMode(string logFileName)
        {
            var logDirectory = Environment.CurrentDirectory;

            CreateDirectoryIfNotExists(logDirectory);

            return Path.Combine(
                logDirectory,
                logFileName);
        }

        private static void CreateDirectoryIfNotExists(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException("Directory cannot be null or empty", nameof(directory));
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}

using System;
using System.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace RoadCaptain.App.RouteBuilder
{
    public class LoggerBootstrapper
    {
        private const string CompanyName = "Codenizer BV";
        private const string ApplicationName = "RoadCaptain";

        public static Logger CreateLogger()
        {
            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug();
            
            // In debug builds always write to the current directory for simplicity sake
            // as that makes the log file easier to pick up from bin\Debug
            var logFilePath = $"roadcaptain-log-{DateTime.UtcNow:yyyy-MM-ddTHHmmss}.log";
            
#if !DEBUG
            logFilePath = CreateLoggerForReleaseMode(logFilePath);

            loggerConfiguration = loggerConfiguration
                .MinimumLevel.Information();
#else
            loggerConfiguration = loggerConfiguration
                .WriteTo.Debug(LogEventLevel.Debug);
#endif

            return loggerConfiguration
                .WriteTo.File(logFilePath, LogEventLevel.Debug)
                .CreateLogger();
        }

        // ReSharper disable once UnusedMember.Local
        private static string CreateLoggerForReleaseMode(string logFileName)
        {
            // Because we install into Program Files (x86) we can't write a log file
            // there when running as a regular user. Good Windows citizenship also
            // means we should write data to the right place which is in the user
            // AppData folder.
            #if WIN
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            #elif MACOS
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            #elif LINUX
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            #else
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            #endif

            CreateDirectoryIfNotExists(Path.Combine(localAppDataFolder, CompanyName));
            CreateDirectoryIfNotExists(Path.Combine(localAppDataFolder, CompanyName, ApplicationName));
            
            var logFilePath = Path.Combine(
                localAppDataFolder,
                CompanyName, 
                ApplicationName, 
                logFileName);

            return logFilePath;
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
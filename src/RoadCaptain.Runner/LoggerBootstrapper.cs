using System;
using System.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace RoadCaptain.Runner
{
    public class LoggerBootstrapper
    {
        private const string CompanyName = "Codenizer BV";
        private const string ApplicationName = "RoadCaptain";

        public static Logger CreateLogger()
        {
            var loggerConfiguration = new LoggerConfiguration().Enrich.FromLogContext();
            var logFileName = $"roadcaptain-log-{DateTime.UtcNow:yyyy-MM-ddTHHmmss}.log";
            
            // In debug builds always write to the current directory for simplicity sake
            // as that makes the log file easier to pick up from bin\Debug
            var logFilePath = logFileName;
            
#if !DEBUG
            logFilePath = CreateLoggerForReleaseMode(logFileName);
#endif

            return loggerConfiguration
                .WriteTo.Debug(LogEventLevel.Debug)
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
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            
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
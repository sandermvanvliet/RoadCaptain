using System;
using Serilog;
using Serilog.Core;

namespace RoadCaptain.Host.Console
{
    internal class MonitoringEventsWithSerilog : MonitoringEvents
    {
        private readonly ILogger _logger;

        public MonitoringEventsWithSerilog()
        {
            _logger = CreateLogger();
        }

        public static Logger CreateLogger()
        {
            return new LoggerConfiguration()
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        public override void Information(string messageTemplate, params object[] propertyValues)
        {
            _logger.Information(messageTemplate, propertyValues);
        }

        public override void Warning(string messageTemplate, params object[] propertyValues)
        {
            _logger.Warning(messageTemplate, propertyValues);
        }

        public override void Error(string messageTemplate, params object[] propertyValues)
        {
            _logger.Error(messageTemplate, propertyValues);
        }

        public override void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.Error(exception, messageTemplate, propertyValues);
        }
    }
}
using System;
using Serilog;

namespace RoadCaptain.Host.Console
{
    internal class MonitoringEventsWithSerilog : MonitoringEvents
    {
        private readonly ILogger _logger;

        public MonitoringEventsWithSerilog(ILogger logger)
        {
            _logger = logger;
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
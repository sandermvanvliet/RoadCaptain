// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Serilog;
using Serilog.Events;

namespace RoadCaptain.App.RouteBuilder
{
    public class MonitoringEventsWithSerilog : MonitoringEvents
    {
        private readonly ILogger _logger;

        public MonitoringEventsWithSerilog(ILogger logger)
        {
            _logger = logger;
        }

        public override void Debug(string message, params object[] arguments)
        {
            if (_logger.IsEnabled(LogEventLevel.Debug))
            {
                _logger.Debug(message, arguments);
            }
        }

        public override void Information(string messageTemplate, params object[] propertyValues)
        {
            _logger.Information(messageTemplate, propertyValues);
        }

        public override void Warning(string messageTemplate, params object[] propertyValues)
        {
            _logger.Warning(messageTemplate, propertyValues);
        }

        public override void Warning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.Warning(exception, messageTemplate, propertyValues);
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

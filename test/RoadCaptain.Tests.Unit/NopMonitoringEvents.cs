using System;

namespace RoadCaptain.Tests.Unit
{
    public class NopMonitoringEvents : MonitoringEvents
    {
        public override void Information(string message, params object[] arguments)
        {
        }

        public override void Warning(string message, params object[] arguments)
        {
        }

        public override void Error(string message, params object[] arguments)
        {
        }

        public override void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }
    }
}
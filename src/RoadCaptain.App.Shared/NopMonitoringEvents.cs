// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain.App.Shared
{
    public class NopMonitoringEvents : MonitoringEvents
    {
        public override void Debug(string message, params object[] arguments)
        {
        }

        public override void Information(string message, params object[] arguments)
        {
        }

        public override void Warning(string message, params object[] arguments)
        {
        }

        public override void Warning(Exception exception, string message, params object[] arguments)
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

// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Reflection;
using RoadCaptain.App.Shared;

namespace RoadCaptain.App.RouteBuilder
{
    public static class MonitoringEventsExtensions
    {
        public static void ApplicationStarted(this MonitoringEvents monitoringEvents)
        {
            monitoringEvents.Information("RoadCaptain Route Builder version {@Version}", ApplicationDiagnosticInformation.GetFrom(Assembly.GetExecutingAssembly()));
        }

        public static void ApplicationStopping(this MonitoringEvents monitoringEvents)
        {
            monitoringEvents.Information("RoadCaptain Route Builder stopping...");
        }

        public static void ApplicationStopped(this MonitoringEvents monitoringEvents)
        {
            monitoringEvents.Information("RoadCaptain Route Builder stopped");
        }
    }
}

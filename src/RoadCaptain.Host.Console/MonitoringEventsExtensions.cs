// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Reflection;

namespace RoadCaptain.Host.Console
{
    public static class MonitoringEventsExtensions
    {
        public static void ApplicationStarted(this MonitoringEvents monitoringEvents)
        {
            var version = GetApplicationVersion();

            monitoringEvents.Information("RoadCaptain version {Version}", version);
        }

        public static void ApplicationStopping(this MonitoringEvents monitoringEvents)
        {
            monitoringEvents.Information("RoadCaptain stopping...");
        }

        public static void ApplicationStopped(this MonitoringEvents monitoringEvents)
        {
            monitoringEvents.Information("RoadCaptain stopped");
        }

        public static void ServiceStarted(this MonitoringEvents monitoringEvents, string serviceName)
        {
            monitoringEvents.Information("Service {Name} started", serviceName);
        }

        public static void ServiceStopped(this MonitoringEvents monitoringEvents, string serviceName)
        {
            monitoringEvents.Information("Service {Name} stopped", serviceName);
        }

        private static string GetApplicationVersion()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();

            var assemblyName = executingAssembly.GetName();

            if (assemblyName.Version == null)
            {
                return "0.0.0.1";
            }

            return assemblyName.Version.ToString(4);
        }
    }
}

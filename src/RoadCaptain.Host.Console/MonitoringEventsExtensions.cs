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

        public static void ApplicationEnded(this MonitoringEvents monitoringEvents)
        {
            monitoringEvents.Information("RoadCaptain exiting...");
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
// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Reflection;
using RoadCaptain.GameStates;

namespace RoadCaptain.App.Runner
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

        public static void StateTransition(this MonitoringEvents monitoringEvents, GameState previousGameState,
            GameState gameState)
        {
            monitoringEvents.Debug(
                "Transitioning from '{CurrentGameState}' to '{NewGameState}'",
                previousGameState?.GetType().Name ?? "initial",
                gameState.GetType().Name);
        }

        public static void RouteLoaded(this MonitoringEvents monitoringEvents, PlannedRoute route)
        {
            monitoringEvents.Information("Loaded route {RouteName}, Zwift route: {ZwiftRoute} ({ZwiftWorld})", route.Name, route.ZwiftRouteName, route.World.Name);
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

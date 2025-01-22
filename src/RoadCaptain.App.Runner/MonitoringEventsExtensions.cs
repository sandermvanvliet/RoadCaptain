// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Reflection;
using RoadCaptain.App.Shared;
using RoadCaptain.GameStates;

namespace RoadCaptain.App.Runner
{
    public static class MonitoringEventsExtensions
    {
        public static void ApplicationStarted(this MonitoringEvents monitoringEvents)
        {
            monitoringEvents.Information("RoadCaptain Runner version {@Version}", ApplicationDiagnosticInformation.GetFrom(Assembly.GetExecutingAssembly()));
        }

        public static void ApplicationStopping(this MonitoringEvents monitoringEvents)
        {
            monitoringEvents.Information("RoadCaptain Runner stopping...");
        }

        public static void ApplicationStopped(this MonitoringEvents monitoringEvents)
        {
            monitoringEvents.Information("RoadCaptain Runner stopped");
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
            monitoringEvents.Information("Loaded route {RouteName}, Zwift route: {ZwiftRoute} ({ZwiftWorld})",
                route.Name ?? "(unknown)",
                route.ZwiftRouteName ?? "(unknown)",
                route.World?.Name ?? "(unknown)");
        }
    }
}

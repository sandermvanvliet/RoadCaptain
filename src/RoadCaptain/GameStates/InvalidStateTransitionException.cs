// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain.GameStates
{
    public class InvalidStateTransitionException : Exception
    {
        public InvalidStateTransitionException(string message)
            : base(message)
        {
        }

        private InvalidStateTransitionException(Type fromState, Type toState, string because)
            : base($"Cannot transition from {fromState.Name} to {toState.Name} because {because}")
        {
        }

        public static InvalidStateTransitionException NotOnARoute(Type fromState) =>
            new(
                fromState, 
                typeof(UpcomingTurnState),
                "the rider is not on a route");

        public static InvalidStateTransitionException AlreadyInGame(Type fromState) =>
            new(
                fromState, 
                typeof(InGameState),
                "the rider is already in a game");

        public static InvalidStateTransitionException NotInGame(Type fromState) =>
            new(
                fromState, 
                typeof(InGameState),
                "the rider is not (yet) in game");

        public static InvalidStateTransitionException NotLoggedIn(Type fromState) =>
            new(
                fromState, 
                typeof(InGameState),
                "the rider is not logged in");

        public static InvalidStateTransitionException RouteNotStarted(Type fromState) => 
            new (
                fromState,
                typeof(OnRouteState),
                "Can't be on-route if the route hasn't started");

        public static InvalidStateTransitionException RouteCompleted(Type fromState) =>
            new(
                fromState,
                typeof(OnRouteState),
                "Can't be on-route if the route has been completed");
    }
}

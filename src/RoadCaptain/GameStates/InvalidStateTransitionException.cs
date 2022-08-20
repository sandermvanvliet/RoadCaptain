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
    }
}
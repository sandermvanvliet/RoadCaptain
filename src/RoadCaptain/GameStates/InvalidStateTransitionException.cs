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

        public static InvalidStateTransitionException NotOnARouteYet(Type fromState) =>
            new(
                fromState, 
                typeof(UpcomingTurnState),
                "the rider is not on a route (yet)");

        public static InvalidStateTransitionException AlreadyInGame(Type fromState) =>
            new(
                fromState, 
                typeof(InGameState),
                "the rider is already in a game");
    }
}
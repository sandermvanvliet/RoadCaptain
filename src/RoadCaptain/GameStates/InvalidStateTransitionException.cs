using System;

namespace RoadCaptain.GameStates
{
    public class InvalidStateTransitionException : Exception
    {
        public InvalidStateTransitionException(string message)
            : base(message)
        {
        }

        public InvalidStateTransitionException(Type fromState, Type toState)
            : base($"Cannot transition from {fromState.Name} to {toState.Name}")
        {
        }
    }
}
using System;

namespace RoadCaptain.GameStates
{
    public class InvalidStateTransitionException : Exception
    {
        public InvalidStateTransitionException(string message)
            : base(message)
        {
        }
    }
}
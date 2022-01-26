using System.Collections.Generic;
using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    internal class InMemoryMessageEmitter : IMessageEmitter
    {
        public InMemoryMessageEmitter()
        {
            Messages = new List<object>();
        }

        public List<object> Messages { get; }

        public void Emit(object message)
        {
            Messages.Add(message);
        }
    }
}
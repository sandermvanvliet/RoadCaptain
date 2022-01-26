using System;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class MessageEmitterToQueue : IMessageEmitter
    {
        public void Emit(object message)
        {
            throw new NotImplementedException();
        }
    }
}
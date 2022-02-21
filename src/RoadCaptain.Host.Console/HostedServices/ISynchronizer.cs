using System;
using System.Threading.Tasks;

namespace RoadCaptain.Host.Console.HostedServices
{
    public interface ISynchronizer
    {
        /// <summary>
        /// Register a callback to invoke when the synchronization event (<see cref="TriggerSynchronizationEvent"/>) happens
        /// </summary>
        /// <param name="func"></param>
        void RegisterStart(Func<Task> func);

        /// <summary>
        /// Register a callback to invoke when a stop of the application is requested
        /// </summary>
        /// <param name="action"></param>
        void RegisterStop(Action action);
        
        void TriggerSynchronizationEvent();
        
        void RequestApplicationStop();
    }
}
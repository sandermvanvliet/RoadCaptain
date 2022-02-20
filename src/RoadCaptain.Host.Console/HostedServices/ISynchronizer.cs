using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoadCaptain.Host.Console.HostedServices
{
    internal interface ISynchronizer
    {
        /// <summary>
        /// Register a callback to invoke when the synchronization event (<see cref="TriggerSynchronizationEvent"/>) happens
        /// </summary>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        void RegisterStart(Func<CancellationToken, Task> func, CancellationToken cancellationToken);

        /// <summary>
        /// Register a callback to invoke when a stop of the application is requested
        /// </summary>
        /// <param name="action"></param>
        void RegisterStop(Action action);
        
        void TriggerSynchronizationEvent();
        
        void RequestApplicationStop();
    }
}
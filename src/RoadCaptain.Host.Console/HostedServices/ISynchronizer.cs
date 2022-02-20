using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoadCaptain.Host.Console.HostedServices
{
    internal interface ISynchronizer
    {
        void RegisterStart(Func<CancellationToken, Task> func, CancellationToken cancellationToken);
        void RegisterStop(Action action);
        void Start();
        void Stop();
    }
}
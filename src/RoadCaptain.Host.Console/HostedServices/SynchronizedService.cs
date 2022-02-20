using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace RoadCaptain.Host.Console.HostedServices
{
    /// <summary>
    /// Base type for hosted services that need to be delay-started whenever
    /// a synchronization event happens
    /// </summary>
    internal abstract class SynchronizedService : IHostedService
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly ISynchronizer _synchronizer;

        protected abstract string Name { get; }

        protected SynchronizedService(MonitoringEvents monitoringEvents, ISynchronizer synchronizer)
        {
            _monitoringEvents = monitoringEvents;
            _synchronizer = synchronizer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Register with the synchronizer, that will
            // call the actual start when the synchronization
            // event happens.
            _synchronizer.RegisterStart(SynchronizedStartCoreAsync, cancellationToken);

            return Task.CompletedTask;
        }

        // This method is a shim only to ensure a consistent
        // interface is presented to classes that derive from this.
        // Otherwise we'd end up with StartCoreAsync() and StopAsync()
        // to implement which just looks weird.
        [DebuggerStepThrough]
        public Task StopAsync(CancellationToken cancellationToken)
        {
            var task = StopCoreAsync(cancellationToken);

            _monitoringEvents.ServiceStopped(Name);

            return task;
        }

        private async Task SynchronizedStartCoreAsync(CancellationToken cancellationToken)
        {
            await StartCoreAsync(cancellationToken);

            _monitoringEvents.ServiceStarted(Name);
        }

        protected abstract Task StartCoreAsync(CancellationToken cancellationToken);
        
        protected abstract Task StopCoreAsync(CancellationToken cancellationToken);
    }
}
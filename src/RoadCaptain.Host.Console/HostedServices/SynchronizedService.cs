// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace RoadCaptain.Host.Console.HostedServices
{
    /// <summary>
    /// Base type for hosted services that should delay starting until signalled by the <see cref="ISynchronizer"/>
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

        /// <inheritdoc cref="IHostedService.StartAsync"/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Register with the synchronizer, that will
            // call the actual start when the synchronization
            // event happens.
            _synchronizer.RegisterStart(async () =>
            {
                await StartCoreAsync(cancellationToken);

                _monitoringEvents.ServiceStarted(Name);
            });

            return Task.CompletedTask;
        }

        /// <inheritdoc cref="IHostedService.StopAsync"/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Prevent re-entry
            if (IsStopped)
            {
                return Task.CompletedTask;
            }

            IsStopped = true;

            var task = StopCoreAsync(cancellationToken);

            _monitoringEvents.ServiceStopped(Name);

            return task;
        }

        public bool IsStopped { get; private set; }

        protected abstract Task StartCoreAsync(CancellationToken cancellationToken);
        
        protected abstract Task StopCoreAsync(CancellationToken cancellationToken);
    }
}

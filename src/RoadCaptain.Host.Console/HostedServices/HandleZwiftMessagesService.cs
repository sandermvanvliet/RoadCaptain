// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.UseCases;

namespace RoadCaptain.Host.Console.HostedServices
{
    internal class HandleZwiftMessagesService : SynchronizedService
    {
        private readonly HandleZwiftMessagesUseCase _useCase;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public HandleZwiftMessagesService(MonitoringEvents monitoringEvents,
            HandleZwiftMessagesUseCase useCase, ISynchronizer synchronizer)
        :base(monitoringEvents, synchronizer)
        {
            _useCase = useCase;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected override string Name => nameof(HandleZwiftMessagesUseCase);

        protected override Task StartCoreAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(
                () => _useCase.Execute(_cancellationTokenSource.Token),
                _cancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        protected override Task StopCoreAsync(CancellationToken cancellationToken)
        {
            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch (OperationCanceledException)
            {
            }

            return Task.CompletedTask;
        }
    }
}

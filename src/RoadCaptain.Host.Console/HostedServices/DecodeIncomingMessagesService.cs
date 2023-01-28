// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.UseCases;

namespace RoadCaptain.Host.Console.HostedServices
{
    internal class DecodeIncomingMessagesService : SynchronizedService
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly DecodeIncomingMessagesUseCase _incomingMessagesUseCase;

        public DecodeIncomingMessagesService(MonitoringEvents monitoringEvents,
            DecodeIncomingMessagesUseCase incomingMessagesUseCase, ISynchronizer synchronizer)
        :base(monitoringEvents, synchronizer)
        {
            _incomingMessagesUseCase = incomingMessagesUseCase;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected override string Name => nameof(DecodeIncomingMessagesService);

        protected override Task StartCoreAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(async () => await _incomingMessagesUseCase.ExecuteAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
            
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

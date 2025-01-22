// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.Commands;
using RoadCaptain.UseCases;

namespace RoadCaptain.Host.Console.HostedServices
{
    internal class ConnectToZwiftService : SynchronizedService
    {
        private readonly ConnectToZwiftUseCase _useCase;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Configuration _configuration;

        public ConnectToZwiftService(MonitoringEvents monitoringEvents,
            ConnectToZwiftUseCase useCase, Configuration configuration, ISynchronizer synchronizer) : base(monitoringEvents, synchronizer)
        {
            _useCase = useCase;
            _configuration = configuration;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected override string Name => nameof(ConnectToZwiftService);

        protected override Task StartCoreAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(async () => await _useCase.ExecuteAsync(new ConnectCommand
                    {
                        AccessToken = _configuration.AccessToken
                    },
                    _cancellationTokenSource.Token),
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

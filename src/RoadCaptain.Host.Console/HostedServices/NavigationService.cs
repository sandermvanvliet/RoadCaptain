using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;

namespace RoadCaptain.Host.Console.HostedServices
{
    internal class NavigationService: IHostedService
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly NavigationUseCase _useCase;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IGameStateDispatcher _gameStateDispatcher;

        public NavigationService(
            MonitoringEvents monitoringEvents,
            NavigationUseCase useCase, 
            IGameStateDispatcher gameStateDispatcher)
        {
            _monitoringEvents = monitoringEvents;
            _useCase = useCase;
            _gameStateDispatcher = gameStateDispatcher;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(
                () => _useCase.Execute(_cancellationTokenSource.Token),
                _cancellationTokenSource.Token);

            _monitoringEvents.ServiceStarted(nameof(NavigationService));
            
            var route = new SegmentSequenceBuilder()
                .StartingAt("watopia-bambino-fondo-001-after-after-after-after-after")
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-after-before-after")
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-after-before-before")
                // Lap 1
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-before")
                .GoingStraightTo("watopia-bambino-fondo-002-after")
                .TuringLeftTo("watopia-beach-island-loop-004")
                .TuringLeftTo("watopia-bambino-fondo-001-after-after-after-after-before-before")
                // Lap 2
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-before")
                .GoingStraightTo("watopia-bambino-fondo-002-after")
                .TuringLeftTo("watopia-beach-island-loop-004")
                .TuringLeftTo("watopia-bambino-fondo-001-after-after-after-after-before-before")
                // Lap 3
                .GoingStraightTo("watopia-bambino-fondo-001-after-after-after-before")
                .GoingStraightTo("watopia-bambino-fondo-002-after")
                .TuringLeftTo("watopia-beach-island-loop-004")
                .TuringLeftTo("watopia-bambino-fondo-001-after-after-after-after-before-before")
                .TurningRightTo("watopia-bambino-fondo-004-before-before")
                .EndingAt("watopia-bambino-fondo-004-before-before")
                .Build();

            _gameStateDispatcher.RouteSelected(route);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _monitoringEvents.ServiceStopped(nameof(NavigationService));
            }

            return Task.CompletedTask;
        }
    }
}

using System.Threading;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class HandleZwiftMessagesUseCase
    {
        private readonly IMessageEmitter _emitter;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IMessageReceiver _messageReceiver;
        private bool _pingedBefore;
        private static readonly object SyncRoot = new();
        private readonly HandleRiderPositionUseCase _handleRiderPositionUseCase;
        private readonly HandleAvailableTurnsUseCase _handleAvailableTurnsUseCase;
        private readonly HandleActivityDetailsUseCase _handleActivityDetailsUseCase;

        public HandleZwiftMessagesUseCase(
            IMessageEmitter emitter,
            MonitoringEvents monitoringEvents,
            IMessageReceiver messageReceiver, 
            HandleRiderPositionUseCase handleRiderPositionUseCase, 
            HandleAvailableTurnsUseCase handleAvailableTurnsUseCase, 
            HandleActivityDetailsUseCase handleActivityDetailsUseCase)
        {
            _emitter = emitter;
            _monitoringEvents = monitoringEvents;
            _messageReceiver = messageReceiver;
            _handleRiderPositionUseCase = handleRiderPositionUseCase;
            _handleAvailableTurnsUseCase = handleAvailableTurnsUseCase;
            _handleActivityDetailsUseCase = handleActivityDetailsUseCase;
        }

        public void Execute(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Dequeue will block if there are no messages in the queue
                var message = _emitter.Dequeue(token);

                if (message is ZwiftRiderPositionMessage riderPosition)
                {
                    _monitoringEvents.RiderPositionReceived(riderPosition.Latitude, riderPosition.Longitude, riderPosition.Altitude);

                    // Convert from Zwift game coordinates to a lat/lon coordinate
                    var position = TrackPoint.FromGameLocation((decimal)riderPosition.Latitude, (decimal)riderPosition.Longitude, (decimal)riderPosition.Altitude);

                    _handleRiderPositionUseCase.Execute(position);
                }
                else if (message is ZwiftPingMessage ping)
                {
                    HandlePingMessage(ping);
                }
                else if (message is ZwiftCommandAvailableMessage commandAvailable)
                {
                    _handleAvailableTurnsUseCase.Execute(commandAvailable);
                }
                else if (message is ZwiftPowerUpMessage powerUp)
                {
                    _monitoringEvents.PowerUpAvailable(powerUp.Type);
                }
                else if (message is ZwiftActivityDetailsMessage activityDetails)
                {
                    _handleActivityDetailsUseCase.Execute(activityDetails);
                }
            }
        }

        private void HandlePingMessage(ZwiftPingMessage ping)
        {
            if (!_pingedBefore)
            {
                lock (SyncRoot)
                {
                    if (_pingedBefore)
                    {
                        return;
                    }

                    _pingedBefore = true;
                }

                _messageReceiver.SendInitialPairingMessage(ping.RiderId);
            }
        }
    }
}
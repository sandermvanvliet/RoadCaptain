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

        public HandleZwiftMessagesUseCase(IMessageEmitter emitter, MonitoringEvents monitoringEvents, IMessageReceiver messageReceiver)
        {
            _emitter = emitter;
            _monitoringEvents = monitoringEvents;
            _messageReceiver = messageReceiver;
        }

        public void Execute(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Dequeue will block if there are no messages in the queue
                var message  = _emitter.Dequeue(token);

                if (message is ZwiftRiderPositionMessage riderPosition)
                {
                    _monitoringEvents.RiderPositionReceived(riderPosition.Latitude, riderPosition.Longitude);
                }
                else if (message is ZwiftPingMessage ping)
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
                else if (message is ZwiftCommandAvailableMessage commandAvailable)
                {
                        _monitoringEvents.CommandAvailable(commandAvailable.Type);
                }
                else if (message is ZwiftPowerUpMessage powerUp)
                {
                    _monitoringEvents.PowerUpAvailable(powerUp.Type);
                }
            }
        }
    }
}
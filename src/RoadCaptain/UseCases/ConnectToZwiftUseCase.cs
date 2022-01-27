using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.Commands;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class ConnectToZwiftUseCase
    {
        private readonly IRequestToken _requestToken;
        private readonly IZwift _zwift;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IMessageReceiver _messageReceiver;
        private bool _pingedBefore;
        private static readonly object SyncRoot = new();

        public ConnectToZwiftUseCase(
            IRequestToken requestToken, 
            IZwift zwift, 
            MonitoringEvents monitoringEvents, 
            IMessageReceiver messageReceiver)
        {
            _requestToken = requestToken;
            _zwift = zwift;
            _monitoringEvents = monitoringEvents;
            _messageReceiver = messageReceiver;
        }

        public async Task ExecuteAsync(ConnectCommand connectCommand, CancellationToken cancellationToken)
        {
            // TODO: Work out what the correct IP address should be
            var ipAddress = "192.168.1.70";

            var tokens = await _requestToken.RequestAsync(connectCommand.Username, connectCommand.Password);

            var relayUri = await _zwift.RetrieveRelayUrl(tokens.AccessToken);
            
            await _zwift.InitiateRelayAsync(tokens.AccessToken, relayUri, ipAddress);

            while (!cancellationToken.IsCancellationRequested)
            {
                // Check whether user is currently in-game
                // If not, sleep for a while and try again

                var profile = await _zwift.GetProfileAsync(tokens.AccessToken);

                if (profile != null && profile.Riding)
                {
                    _monitoringEvents.UserIsRiding();
                    break;
                }

                Thread.Sleep(5 * 1000);
            }
        }

        private void HandlePing(int riderId)
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

                _messageReceiver.SendInitialPairingMessage(riderId);
            }
        }
    }
}
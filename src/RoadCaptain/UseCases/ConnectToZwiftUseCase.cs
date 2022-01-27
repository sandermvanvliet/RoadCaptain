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

        public ConnectToZwiftUseCase(
            IRequestToken requestToken, 
            IZwift zwift, 
            MonitoringEvents monitoringEvents)
        {
            _requestToken = requestToken;
            _zwift = zwift;
            _monitoringEvents = monitoringEvents;
        }

        public async Task ExecuteAsync(ConnectCommand connectCommand, CancellationToken cancellationToken)
        {
            var ipAddress = "192.168.1.53";

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
    }
}
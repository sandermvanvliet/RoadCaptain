using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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
            // TODO: Work out what the correct IP address should be
            var ipAddress = GetMostLikelyAddress().ToString();

            _monitoringEvents.Information("Telling Zwift to connect to {IPAddress}:21587", ipAddress);

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

        private static IPAddress GetMostLikelyAddress()
        {
            // Only look at network interfaces that are either Ethernet or WiFi
            var nics = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                              nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                .ToList();

            var likelyAddresses = new List<IPAddress>();

            foreach (var nic in nics)
            {
                var ipProperties = nic.GetIPProperties();

                // If there is no gateway set then the network interface
                // most likely isn't the right one as it's probably not
                // routable anyway.
                if (!ipProperties.GatewayAddresses.Any())
                {
                    continue;
                }

                var likelyAddress = ipProperties
                        .UnicastAddresses
                        .Where(unicastAddress => unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                        .Select(unicastAddress => unicastAddress.Address)
                        .FirstOrDefault();

                if (likelyAddress != null)
                {
                    likelyAddresses.Add(likelyAddress);
                }
            }

            return likelyAddresses.FirstOrDefault();
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.Commands;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class ConnectToZwiftUseCase
    {
        private readonly IRequestToken _requestToken;
        private readonly IZwift _zwift;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IGameStateReceiver _gameStateReceiver;
        private bool _userIsInGame;

        public ConnectToZwiftUseCase(IRequestToken requestToken,
            IZwift zwift,
            MonitoringEvents monitoringEvents, 
            IGameStateReceiver gameStateReceiver)
        {
            _requestToken = requestToken;
            _zwift = zwift;
            _monitoringEvents = monitoringEvents;
            _gameStateReceiver = gameStateReceiver;
            _gameStateReceiver.Register(
                null,
                null,
                ReceiveGameState);
        }

        private void ReceiveGameState(GameState gameState)
        {
            if (gameState is InGameState && !_userIsInGame)
            {
                _userIsInGame = true;
            }
            else if (gameState is NotInGameState && _userIsInGame)
            {
                _userIsInGame = false;
            }
        }

        public async Task ExecuteAsync(ConnectCommand connectCommand, CancellationToken cancellationToken)
        {
            // Listen for game state updates
#pragma warning disable CS4014
            // ReSharper disable once MethodSupportsCancellation
            Task.Factory.StartNew(() => _gameStateReceiver.Start(cancellationToken));
#pragma warning restore CS4014

            // TODO: Work out what the correct IP address should be
            var ipAddress = GetMostLikelyAddress().ToString();

            _monitoringEvents.Information("Telling Zwift to connect to {IPAddress}:21587", ipAddress);

            var tokens = await _requestToken.RequestAsync(connectCommand.Username, connectCommand.Password);

            var relayUri = await _zwift.RetrieveRelayUrl(tokens.AccessToken);

            await _zwift.InitiateRelayAsync(tokens.AccessToken, relayUri, ipAddress);

            var remainingAttempts = 5;

            while (!cancellationToken.IsCancellationRequested)
            {
                // _userIsInGame is set through a game state update
                // which we receive directly on a connection with Zwift.
                // That means that if it is set we can exit.
                if (_userIsInGame)
                {
                    _monitoringEvents.UserIsRiding();
                    break;
                }

                // Check whether user is currently in-game
                // If not, sleep for a while and try again
                var profile = await _zwift.GetProfileAsync(tokens.AccessToken);

                if (profile != null && profile.Riding)
                {
                    _monitoringEvents.UserIsRiding();
                    break;
                }

                remainingAttempts--;

                if (remainingAttempts <= 0 && !_userIsInGame)
                {
                    _monitoringEvents.Warning("Zwift did not connect, attempting link again on {IPAddress}:21587", ipAddress);

                    await _zwift.InitiateRelayAsync(tokens.AccessToken, relayUri, ipAddress);

                    remainingAttempts = 5;
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
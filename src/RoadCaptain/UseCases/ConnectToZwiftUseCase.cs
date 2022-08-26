using System;
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
        private readonly IZwift _zwift;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IGameStateReceiver _gameStateReceiver;
        private readonly IGameStateDispatcher _gameStateDispatcher;
        private bool _userIsInGame;
        private readonly AutoResetEvent _userDisconnected = new(false);

        public ConnectToZwiftUseCase(IZwift zwift,
            MonitoringEvents monitoringEvents, 
            IGameStateReceiver gameStateReceiver,
            IGameStateDispatcher gameStateDispatcher)
        {
            _zwift = zwift;
            _monitoringEvents = monitoringEvents;
            _gameStateReceiver = gameStateReceiver;
            _gameStateDispatcher = gameStateDispatcher;
            _gameStateReceiver.Register(
                null,
                null,
                ReceiveGameState);
        }

        private void ReceiveGameState(GameState gameState)
        {
            // TODO: Revisit this
            if (GameState.IsInGame(gameState) && !_userIsInGame)
            {
                _userIsInGame = true;
            }
            else if (gameState is ConnectedToZwiftState && _userIsInGame)
            {
                _userIsInGame = false;
                _userDisconnected.Set();
            }
            else if (gameState is WaitingForConnectionState && _userIsInGame)
            {
                _userIsInGame = false;
                _userDisconnected.Set();
            }
        }

        public async Task ExecuteAsync(ConnectCommand connectCommand, CancellationToken cancellationToken)
        {
            // Listen for game state updates
#pragma warning disable CS4014
            // ReSharper disable once MethodSupportsCancellation
            Task.Factory.StartNew(() => _gameStateReceiver.Start(cancellationToken));
#pragma warning restore CS4014

            // To ensure that we don't block a long time 
            // when there are no items in the queue we
            // need to trigger the auto reset event when
            // the token is cancelled.
            cancellationToken.Register(() => _userDisconnected.Set());

            // TODO: Work out what the correct IP address should be
            var ipAddress = GetMostLikelyAddress().ToString();

            var zwiftConnectionPort = connectCommand.ConnectionEncryptionSecret == null ? 21587 : 21588;

            _monitoringEvents.Information("Telling Zwift to connect to {IPAddress}:{Port}", ipAddress, zwiftConnectionPort);

            Uri relayUri;

            try
            {
                relayUri = await _zwift.RetrieveRelayUrl(connectCommand.AccessToken);

                await _zwift.InitiateRelayAsync(connectCommand.AccessToken, relayUri, ipAddress, connectCommand.ConnectionEncryptionSecret);
            }
            catch (Exception e)
            {
                _gameStateDispatcher.Dispatch(new InvalidCredentialsState(e));
                return;
            }

            var remainingAttempts = 5;

            try
            {
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
                    var profile = await _zwift.GetProfileAsync(connectCommand.AccessToken);

                    if (profile.Riding)
                    {
                        _monitoringEvents.UserIsRiding();
                        break;
                    }

                    remainingAttempts--;

                    if (remainingAttempts <= 0 && !_userIsInGame)
                    {
                        _monitoringEvents.Warning("Zwift did not connect, attempting link again on {IPAddress}:{Port}", ipAddress, zwiftConnectionPort);

                        await _zwift.InitiateRelayAsync(connectCommand.AccessToken, relayUri, ipAddress, connectCommand.ConnectionEncryptionSecret);

                        remainingAttempts = 5;
                    }
                    
                    _userDisconnected.WaitOne(5 * 1000);
                }
            }
            catch (Exception e)
            {
                _gameStateDispatcher.Dispatch(new ErrorState(e));
            }
        }

        private static IPAddress GetMostLikelyAddress()
        {
            // Only look at network interfaces that are either Ethernet or WiFi
            var networkInterfaces = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                              nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                .ToList();

            var likelyAddresses = new List<IPAddress>();

            foreach (var networkInterface in networkInterfaces)
            {
                var ipProperties = networkInterface.GetIPProperties();

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
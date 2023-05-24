// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using RoadCaptain.Commands;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class ConnectToZwiftUseCase
    {
        private readonly IZwift _zwift;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IGameStateDispatcher _gameStateDispatcher;

        public ConnectToZwiftUseCase(IZwift zwift,
            MonitoringEvents monitoringEvents,
            IGameStateDispatcher gameStateDispatcher)
        {
            _zwift = zwift;
            _monitoringEvents = monitoringEvents;
            _gameStateDispatcher = gameStateDispatcher;
        }

        public async Task ExecuteAsync(ConnectCommand connectCommand)
        {
            if (connectCommand.ConnectionEncryptionSecret == null || connectCommand.ConnectionEncryptionSecret.Length == 0)
            {
                throw new ArgumentException("Connection secret must be provided");
            }

            if (string.IsNullOrEmpty(connectCommand.AccessToken))
            {
                throw new ArgumentException("Access token must be provided");
            }
            
            // TODO: Work out what the correct IP address should be
            var ipAddress = GetMostLikelyAddress()?.ToString();

            if (string.IsNullOrEmpty(ipAddress))
            {
                throw new Exception("Unable to determine an IP address to listen on");
            }

            _monitoringEvents.Information("Telling Zwift to connect to {IPAddress}", ipAddress);

            try
            {
                var relayUri = await _zwift.RetrieveRelayUrl(connectCommand.AccessToken);

                await _zwift.InitiateRelayAsync(connectCommand.AccessToken, relayUri, ipAddress, connectCommand.ConnectionEncryptionSecret);
            }
            catch (Exception e)
            {
                _gameStateDispatcher.InvalidCredentials(e);
            }
        }

        private static IPAddress? GetMostLikelyAddress()
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

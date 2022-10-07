// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using RoadCaptain.GameStates;

namespace RoadCaptain
{
    public static class MonitoringEventsExtensions
    {
        public static void UserIsRiding(this MonitoringEvents monitoringEvents)
        {
            monitoringEvents.Information("User is currently riding in Zwift");
        }

        public static void ReceivedMessage(this MonitoringEvents monitoringEvents, int size, long sequenceNumber)
        {
            monitoringEvents.Information("Received a message from Zwift. Sequence no {SequenceNumber}, size {Size}", sequenceNumber, size);
        }

        public static void AcceptedConnection(this MonitoringEvents monitoringEvents,
            IPEndPoint? remoteEndPoint)
        {
            if (remoteEndPoint == null)
            {
                monitoringEvents.Information("Accepted a inbound TCP connection from Zwift");
            }
            else
            {
                monitoringEvents.Information("Accepted a inbound TCP connection from Zwift at {IPAddress}:{Port}",
                    remoteEndPoint.Address.ToString(), remoteEndPoint.Port);
            }
        }

        public static void ReceiveFailed(this MonitoringEvents monitoringEvents, SocketError socketError)
        {
            monitoringEvents.Error("Failed to receive data from socket because {Error}, closing socket", socketError.ToString());
        }

        public static void WaitingForConnection(this MonitoringEvents monitoringEvents)
        {
            monitoringEvents.Information("Waiting for inbound TCP connection");
        }

        public static void RiderPositionReceived(this MonitoringEvents monitoringEvents, float latitude,
            float longitude, float altitude)
        {
            //monitoringEvents.Debug("Received rider position {Latitude} {Longitude} {Altitude}",
            //    latitude.ToString("0.00000000", CultureInfo.InvariantCulture),
            //    longitude.ToString("0.00000000", CultureInfo.InvariantCulture),
            //    altitude.ToString("0.00000000", CultureInfo.InvariantCulture));
        }

        public static void AvailableTurns(this MonitoringEvents monitoringEvents, List<TurnDirection> turns)
        {
            // Turns on a segment are always at least 2
            if (turns.Count >= 2)
            {
                monitoringEvents.Information("Currently available turns are now: {Turns}", string.Join(", ", turns.Select(t => t.ToString())));
            }
        }

        public static void PowerUpAvailable(this MonitoringEvents monitoringEvents, string type)
        {
            monitoringEvents.Debug("Received available power-up {Type}", type);
        }

        public static void InvalidStateTransition(
            this MonitoringEvents monitoringEvents,
            InvalidStateTransitionException ex)
        {
            monitoringEvents.Error(ex, ex.Message);
        }
    }
}


using System;
using System.Globalization;
using System.Net.Sockets;

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

        public static void AcceptedConnection(this MonitoringEvents monitoringEvents)
        {
            monitoringEvents.Information("Accepted a inbound TCP connection from Zwift");
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
            monitoringEvents.Debug("Received rider position {Latitude} {Longitude} {Altitude}",
                latitude.ToString("0.00000000", CultureInfo.InvariantCulture), 
                longitude.ToString("0.00000000", CultureInfo.InvariantCulture),
                altitude.ToString("0.00000000", CultureInfo.InvariantCulture));
        }

        public static void CommandAvailable(this MonitoringEvents monitoringEvents, string type)
        {
            // The SomethingEmpty command is received a _lot_ so ignore that to
            // prevent log spamming
            if (!"somethingempty".Equals(type, StringComparison.InvariantCultureIgnoreCase))
            {
                monitoringEvents.Information("Received available command {Type}", type);
            }
        }

        public static void PowerUpAvailable(this MonitoringEvents monitoringEvents, string type)
        {
            monitoringEvents.Information("Received available power-up {Type}", type);
        }
    }
}

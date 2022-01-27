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

        public static void ReceiveFailed(this MonitoringEvents monitoringEvents)
        {
            monitoringEvents.Error("Failed to receive data from socket, closing socket");
        }

        public static void WaitingForConnection(this MonitoringEvents monitoringEvents)
        {
            monitoringEvents.Error("Waiting for inbound TCP connection");
        }
    }
}

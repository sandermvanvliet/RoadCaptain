using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using RoadCaptain.Adapters.Protobuf;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class MessageReceiverFromSocket : IMessageReceiver
    {
        private readonly Socket _socket;
        private Socket _acceptedSocket;
        private readonly MonitoringEvents _monitoringEvents;
        private uint _commandCounter = 1;

        public MessageReceiverFromSocket(MonitoringEvents monitoringEvents)
        {
            _monitoringEvents = monitoringEvents;
            
            _socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp)
            {
                NoDelay = true
            };
        }

        /*
         * ReceiveMessageBytes
         *
         * The purpose here is to read bytes from an established network connection with
         * the Zwift game.
         *
         * The intent of this method is that it blocks until data is available to be read.
         * That means that we block when:
         *
         * - there is no established connection, we want to Accept() a new one
         * - there is an established connection but no data to Receive()
         *
         * The caller should be able to call ReceiveMessageBytes in a loop and expect
         * that when there are many bytes to be read this method returns as quickly as
         * possible.
         *
         * Therefore we will check if there is an established connection when entering
         * this method, if not we call Accept() and block. If there is we call Receive().
         * If there are pending bytes on the socket we will read all the bytes we can.
         * When no bytes are pending on the socket, Receive() will block.
         *
         * When Receive() fails with a socket error we clear the established connection
         * so that the next call to ReceiveMessageBytes will block again on Accept()
         */
        public byte[] ReceiveMessageBytes()
        {
            // Do we have an established connection?
            if (_acceptedSocket == null)
            {
                // No, try to Accept() a new one
                try
                {
                    if (!_socket.IsBound)
                    {
                        // If the socket hasn't been bound before we also 
                        // assume that it's not listening yet.
                        // This should only happen when ReceiveMessageBytes
                        // is called the first time.
                        _socket.Bind(new IPEndPoint(IPAddress.Any, 21587));
                        _socket.Listen();
                    }
                    
                    _monitoringEvents.WaitingForConnection();
                    
                    _acceptedSocket = _socket.Accept();

                    _monitoringEvents.AcceptedConnection();
                }
                catch (ObjectDisposedException)
                {
                    _monitoringEvents.Warning("Listening socket has been closed");
                    throw new InvalidOperationException("Listening socket has been closed. ReceiveMessageBytes can't be called again");
                }
                catch (SocketException ex)
                {
                    _monitoringEvents.Warning(ex, "Failed to accept new connection because of a socket error {Error}", ex.SocketErrorCode.ToString());
                    throw new InvalidOperationException("Failed to accept new connection because of a socket error. ReceiveMessageBytes can't be called again");
                }
            }

            var buffer = new byte[_acceptedSocket.ReceiveBufferSize];
            var total = new List<byte>(buffer.Length); // At least the same as buffer to prevent reallocations

            while (true)
            {
                // Receive will block until bytes are available to read.
                var received =
                    _acceptedSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None, out var socketError);

                if (socketError != SocketError.Success)
                {
                    if (socketError == SocketError.ConnectionReset)
                    {
                        _monitoringEvents.Information("Zwift closed the connection");
                    }
                    else
                    {
                        // Sonmething went wrong...
                        _monitoringEvents.ReceiveFailed(socketError);
                    }

                    try
                    {
                        _acceptedSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch (SocketException)
                    {
                        // Don't care
                    }

                    try
                    {
                        _acceptedSocket.Close();
                    }
                    catch (SocketException)
                    {
                        // Don't care
                    }

                    // Clear this so that the next call to ReceiveMessageBytes() will block
                    // on accepting a new connection.
                    _acceptedSocket = null;

                    return null;
                }

                if (received == 0)
                {
                    // Nothing more in the buffer
                    break;
                }

                total.AddRange(buffer.Take(received));

                if (received < buffer.Length)
                {
                    // Less bytes in the socket buffer than our buffer which
                    // means this should be everything and we can return the
                    // total received bytes.
                    break;
                }

                // The number of bytes received was exactly our buffer
                // size, so wrap around and try to read some more bytes.
                // When the next Receive() call returns zero bytes read
                // we can return some more bytes.
            }

            return total.ToArray();
        }

        public void Shutdown()
        {
            try
            {
                _acceptedSocket?.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException)
            {
                // Don't care
            }

            try
            {
                _acceptedSocket?.Close();
            }
            catch (SocketException)
            {
                // Don't care
            }

            _acceptedSocket = null;

            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException)
            {
                // Don't care
            }

            try
            {
                _socket.Close();
            }
            catch (SocketException)
            {
                // Don't care
            }
        }

        public void SendMessageBytes(byte[] payload)
        {
            if (_acceptedSocket == null)
            {
                _monitoringEvents.Error("Tried to send data to Zwift but there is no active connection");
                return;
            }

            // Note: For messages to the Zwift app we need to have a 4-byte length prefix instead
            // of the 2-byte one we see on incoming messages...
            var payloadToSend = WrapWithLength(payload);

            var offset = 0;

            while (offset < payloadToSend.Length)
            {
                var sent = _acceptedSocket.Send(payloadToSend, offset, payloadToSend.Length - offset, SocketFlags.None);
                
                _monitoringEvents.Debug("Sent {Count} bytes, {Offset} sent so far of {Total} total payload size", sent, offset, payloadToSend.Length);

                offset += sent;
            }
        }

        public void SendInitialPairingMessage(uint riderId, uint sequenceNumber)
        {
            var message = new ZwiftCompanionToAppRiderMessage
            {
                MyId = riderId,
                Details = new ZwiftCompanionToAppRiderMessage.Types.RiderMessage
                {
                    RiderId = riderId,
                    Tag1 = _commandCounter++,
                    Type = 28
                },
                Sequence = sequenceNumber
            };
            
            SendMessageBytes(message.ToByteArray());
        }

        public void SendTurnCommand(TurnDirection direction, uint sequenceNumber)
        {
            var message = new ZwiftCompanionToAppRiderMessage
            {
                MyId = 3089151, /*riderId*/ // TODO: inject this value
                Details = new ZwiftCompanionToAppRiderMessage.Types.RiderMessage
                {
                    CommandType = (uint)GetCommandTypeForTurnDirection(direction),
                    Tag1 = _commandCounter++, // This is a sequence of the number of commands we've sent to the game
                    Type = 22, // Tag2
                    Tag3 = 0,
                    Tag5 = 0,
                    Tag7 = 0
                },
                Sequence = sequenceNumber // This value is provided via the SomethingEmpty synchronisation command
            };

            SendMessageBytes(message.ToByteArray());
        }

        private static CommandType GetCommandTypeForTurnDirection(TurnDirection direction)
        {
            switch (direction)
            {
                case TurnDirection.Left:
                    return CommandType.TurnLeft;
                case TurnDirection.GoStraight:
                    return CommandType.GoStraight;
                case TurnDirection.Right:
                    return CommandType.TurnRight;
                default:
                    return CommandType.Unknown;
            }
        }

        private static byte[] WrapWithLength(byte[] payload)
        {
            var prefix = BitConverter.GetBytes(payload.Length);

            if (BitConverter.IsLittleEndian)
            {
                prefix = prefix.Reverse().ToArray();
            }

            return prefix
                .Concat(payload)
                .ToArray();
        }
    }

    internal enum CommandType
    {
        Unknown = 0,
        ElbowFlick = 4,
        Wave = 5,
        RideOn = 6,
        SomethingEmpty = 23, // This is the synchronisation event for command sequence numbers that are sent to back to the game
        TurnLeft = 1010,
        GoStraight = 1011,
        TurnRight = 1012,
        DiscardAero = 1030,
        DiscardLightweight = 1034,
        PowerGraph = 1060,
        HeadsUpDisplay = 1081,
    }
}

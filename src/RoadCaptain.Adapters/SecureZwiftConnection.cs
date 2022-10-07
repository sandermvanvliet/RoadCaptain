// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Google.Protobuf;
using RoadCaptain.Adapters.Protobuf;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class SecureZwiftConnection : IMessageReceiver, IZwiftGameConnection
    {
        private Socket? _socket;
        private Socket? _acceptedSocket;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IGameStateDispatcher _gameStateDispatcher;
        private uint _commandCounter = 1;
        private static readonly object SyncRoot = new();
        private readonly IZwiftCrypto _zwiftCrypto;

        public SecureZwiftConnection(
            MonitoringEvents monitoringEvents,
            IGameStateDispatcher gameStateDispatcher,
            IZwiftCrypto zwiftCrypto)
        {
            _monitoringEvents = monitoringEvents;
            _gameStateDispatcher = gameStateDispatcher;
            _zwiftCrypto = zwiftCrypto;

            _socket = CreateListeningSocket();
        }

        private Socket CreateListeningSocket()
        {
            if (_socket == null)
            {
                lock (SyncRoot)
                {
                    _socket = new Socket(
                        AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp)
                    {
                        NoDelay = true
                    };
                }
            }

            return _socket;
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
        public byte[]? ReceiveMessageBytes()
        {
            try
            {
                return ReceiveMessageBytesCore();
            }
            catch (Exception ex)
            {
                _monitoringEvents.Error(ex, "Tried to receive data but it failed");
                return null;
            }
        }

        private byte[]? ReceiveMessageBytesCore()
        {
            // Do we have an established connection?
            if (_acceptedSocket == null)
            {
                // No, try to Accept() a new one
                try
                {
                    _socket ??= CreateListeningSocket();

                    if (!_socket.IsBound)
                    {
                        // If the socket hasn't been bound before we also 
                        // assume that it's not listening yet.
                        // This should only happen when ReceiveMessageBytes
                        // is called the first time.
                        _socket.Bind(new IPEndPoint(IPAddress.Any, 21588));
                        _socket.Listen();
                    }

                    _monitoringEvents.WaitingForConnection();

                    _gameStateDispatcher.WaitingForConnection();

                    _acceptedSocket = _socket.Accept();

                    _gameStateDispatcher.Connected();

                    _monitoringEvents.AcceptedConnection(_acceptedSocket.RemoteEndPoint as IPEndPoint);
                }
                catch (ObjectDisposedException)
                {
                    _monitoringEvents.Warning("Listening socket has been closed");
                    throw new InvalidOperationException(
                        "Listening socket has been closed. ReceiveMessageBytes can't be called again");
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
                {
                    // Shutdown was called which closed the socket
                    return null;
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
                var received = _acceptedSocket
                    .Receive(
                        buffer,
                        0,
                        buffer.Length,
                        SocketFlags.None,
                        out var socketError);

                if (socketError != SocketError.Success)
                {
                    if (socketError == SocketError.ConnectionReset)
                    {
                        _monitoringEvents.Information("Zwift closed the connection");
                    }
                    else
                    {
                        // Something went wrong...
                        _monitoringEvents.ReceiveFailed(socketError);
                    }

                    try
                    {
                        _acceptedSocket?.Shutdown(SocketShutdown.Both);
                    }
                    catch (SocketException)
                    {
                        // Don't care
                    }
                    catch (ObjectDisposedException)
                    {
                        _acceptedSocket = null;
                    }

                    try
                    {
                        _acceptedSocket?.Close();
                    }
                    catch (SocketException)
                    {
                        // Don't care
                    }
                    catch (ObjectDisposedException)
                    {
                        // Don't care
                    }
                    finally
                    {
                        // Clear this so that the next call to ReceiveMessageBytes() will block
                        // on accepting a new connection.
                        _acceptedSocket = null;
                    }

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

        public Task StartAsync()
        {
            return Task.CompletedTask;
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
                if (_socket is { Connected: true })
                {
                    _socket.Shutdown(SocketShutdown.Both);
                }
            }
            catch (SocketException)
            {
                // Don't care
            }

            try
            {
                _socket?.Close();
            }
            catch (SocketException)
            {
                // Don't care
            }
            finally
            {
                _socket = null;
            }
        }

        public event EventHandler? AcceptTimeoutExpired;
        public event EventHandler? DataTimeoutExpired;
        public event EventHandler? ConnectionLost;
        public event EventHandler? ConnectionAccepted;

        private void SendMessageBytes(byte[] payload)
        {
            if (_acceptedSocket == null)
            {
                _monitoringEvents.Error("Tried to send data to Zwift but there is no active connection");
                return;
            }

            // Note: For messages to the Zwift app we need to have a 4-byte length prefix instead
            // of the 2-byte one we see on incoming messages...
            var payloadToSend = WrapWithLength(_zwiftCrypto.Encrypt(payload));

            var offset = 0;

            while (offset < payloadToSend.Length)
            {
                var sent = _acceptedSocket.Send(payloadToSend, offset, payloadToSend.Length - offset, SocketFlags.None);

                _monitoringEvents.Debug("Sent {Count} bytes, {Sent} sent so far of {Total} total payload size", sent, offset + sent, payloadToSend.Length);

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
                    Type = (uint)PhoneToGameCommandType.PairingAs
                },
                Sequence = sequenceNumber
            };

            SendMessageBytes(message.ToByteArray());
        }

        public void SendTurnCommand(TurnDirection direction, ulong sequenceNumber, uint riderId)
        {
            var message = new ZwiftCompanionToAppRiderMessage
            {
                MyId = riderId,
                Details = new ZwiftCompanionToAppRiderMessage.Types.RiderMessage
                {
                    CommandType = (uint)GetCommandTypeForTurnDirection(direction),
                    Tag1 = _commandCounter++, // This is a sequence of the number of commands we've sent to the game
                    Type = (uint)PhoneToGameCommandType.CustomAction, // Tag2
                    Tag3 = 0,
                    Tag5 = 0,
                    Tag7 = 0
                },
                Sequence = (uint)sequenceNumber // This value is provided via the SomethingEmpty synchronization command
            };

            SendMessageBytes(message.ToByteArray());
        }

        public void EndActivity(ulong sequenceNumber, string activityName, uint riderId)
        {
            var message = new ZwiftCompanionToAppRiderMessage
            {
                MyId = riderId,
                Details = new ZwiftCompanionToAppRiderMessage.Types.RiderMessage
                {
                    Tag1 = _commandCounter++, // This is a sequence of the number of commands we've sent to the game
                    Type = (uint)PhoneToGameCommandType.DoneRiding, // Tag2
                    Tag3 = 0,
                    Tag5 = 0,
                    Tag7 = 0,
                    Data = new ZwiftCompanionToAppRiderMessage.Types.RiderMessage.Types.RiderMessageData
                    {
                        Tag1 = 15,
                        SubData = new ZwiftCompanionToAppRiderMessage.Types.RiderMessage.Types.RiderMessageData.Types.RiderMessageSubData
                        {
                            Tag1 = 3,
                            WorldName = activityName,
                            Tag4 = 0
                        }
                    }
                },
                Sequence = (uint)sequenceNumber // This value is provided via the SomethingEmpty synchronization command
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
}


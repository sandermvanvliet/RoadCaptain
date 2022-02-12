using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using RoadCaptain.Adapters.Protobuf;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class MessageReceiverFromSocket : IMessageReceiver
    {
        private readonly Socket _socket;
        private Socket _acceptedSocket;
        private readonly Mutex _mutex = new Mutex(false);
        private readonly TimeSpan _mutextTimeout = TimeSpan.FromMilliseconds(250);
        private readonly MonitoringEvents _monitoringEvents;
        private uint _commandCounter = 1;
        private Task _waitForConnectionTask;

        public MessageReceiverFromSocket(MonitoringEvents monitoringEvents)
        {
            _monitoringEvents = monitoringEvents;
            _socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            
            // TODO: Bind/Listen/Accept should not start from within the constructor
            _socket.Bind(new IPEndPoint(IPAddress.Any, 21587));

            _socket.Listen();

            _waitForConnectionTask = Task.Factory.StartNew(WaitForConnection);
        }

        private void WaitForConnection()
        {
            // TODO: Split accept and reading into separate components
            // That way we can have an accept loop that kicks off new processing use cases
            // and not bother with that thing closing on the same mutex which is why currently
            // we have this time-out here.
            try
            {
                if (_mutex.WaitOne(_mutextTimeout))
                {
                    if (_acceptedSocket != null)
                    {
                        // If an accepted socket exists we should
                        // immediately release the mutex and exit.
                        // Accepting the next connection will happen
                        // automatically when receiving fails and the
                        // socket is cleaned up.
                        // WaitForConnection will be restarted from
                        // that path.
                        _mutex.ReleaseMutex();
                        return;
                    }
                }
                else
                {
                    // We did not get exclusive access, how come?
                    Task.Factory.StartNew(WaitForConnection);
                    return;
                }

                _monitoringEvents.WaitingForConnection();

                // This blocks until a connection is made
                _acceptedSocket = _socket.Accept();

                _monitoringEvents.AcceptedConnection();
            }
            catch (Exception e)
            {
                _monitoringEvents.Error(e, "Failed to accept connection");
            }
            finally
            {
                // Allow ReceiveMessagesBytes to unblock
                _mutex.ReleaseMutex();
            }

        }

        public byte[] ReceiveMessageBytes()
        {
            var buffer = new byte[512];
            var total = new List<byte>();
            
            // Block here until a connection is Accept()-ed and we can read.
            try
            {
                while (!_mutex.WaitOne(_mutextTimeout))
                {
                    // Additional sleep here?
                }
            }
            catch (AbandonedMutexException e)
            {
                _monitoringEvents.Error(e, "Mutex was abandoned, can't continue with receiving data");

                if (_waitForConnectionTask != null &&
                    (_waitForConnectionTask.IsCompleted || _waitForConnectionTask.IsFaulted))
                {
                    RestartWaitForConnectionTask();
                } 
                else if (_waitForConnectionTask == null)
                {
                    // This is weird because _waitForConnectionTask is initialized
                    // in the constructor but hey...
                    RestartWaitForConnectionTask();
                }

                return null;
            }

            if (_acceptedSocket == null)
            {
                return null;
            }

            var socketErrorOccurred = false;

            try
            {
                while (true)
                {
                    // Receive will block until bytes are available to read.
                    var received =
                        _acceptedSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None, out var socketError);

                    if (socketError != SocketError.Success)
                    {
                        socketErrorOccurred = true;

                        // Shit went wrong...
                        _monitoringEvents.ReceiveFailed(socketError);

                        _acceptedSocket.Close();
                        _acceptedSocket = null;

                        RestartWaitForConnectionTask();

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
            finally
            {
                if (socketErrorOccurred)
                {
                    // As we have the lock at this point we can release here and be done.
                    // This _must_ happen after we start the task for WaitFOrConnection
                    // otherwise that will bork because it acquires the lock too soon.
                    _mutex.ReleaseMutex();
                }
            }
        }

        private void RestartWaitForConnectionTask()
        {
            _monitoringEvents.Information("Attempting to restart WaitForConnection task");
            _waitForConnectionTask = Task.Factory.StartNew(WaitForConnection);

            if (_waitForConnectionTask.Status == TaskStatus.Running ||
                _waitForConnectionTask.Status == TaskStatus.WaitingForActivation ||
                _waitForConnectionTask.Status == TaskStatus.WaitingToRun)
            {
                _monitoringEvents.Information("WaitForConnection task restarted successfully");
            }
            else
            {
                _monitoringEvents.Error("Failed to start WaitForConnection task, its state is now {State}",
                    _waitForConnectionTask.Status);
            }
        }

        public void Shutdown()
        {
            _acceptedSocket?.Close();

            _socket.Close();
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

            var memoryStream = new MemoryStream();
            var outputStream = new CodedOutputStream(memoryStream);
            message.WriteTo(outputStream);
            outputStream.Flush(); // Need to flush otherwise GetBuffer() returns an empty array...
            var bytes = memoryStream.GetBuffer().Take((int)memoryStream.Position).ToArray();
            
            SendMessageBytes(bytes);
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
                Sequence = sequenceNumber // No idea how this is counted or if it's an echo of an incoming message perhaps?
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
        SomethingEmpty = 23, // I suspect this is a  "reset" type thing
        TurnLeft = 1010,
        GoStraight = 1011,
        TurnRight = 1012,
        DiscardAero = 1030,
        DiscardLightweight = 1034,
        PowerGraph = 1060,
        HeadsUpDisplay = 1081,
    }
}

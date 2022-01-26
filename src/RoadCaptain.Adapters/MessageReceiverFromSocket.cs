using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class MessageReceiverFromSocket : IMessageReceiver
    {
        private readonly Socket _socket;
        private Socket _acceptedSocket;
        private readonly Mutex _mutex = new Mutex(false);
        private readonly TimeSpan _mutextTimeout = TimeSpan.FromMilliseconds(250);

        public MessageReceiverFromSocket()
        {
            _socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            
            _socket.Bind(new IPEndPoint(IPAddress.Any, 21587));

            _socket.Listen();

            Task.Factory.StartNew(WaitForConnection);
        }

        private void WaitForConnection()
        {
            if (_mutex.WaitOne())
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
                return;
            }

            // This blocks until a connection is made
            _acceptedSocket = _socket.Accept();

            // Allow ReceiveMessagesBytes to unblock
            _mutex.ReleaseMutex();
        }

        public byte[] ReceiveMessageBytes()
        {
            var buffer = new byte[512];
            var total = new List<byte>();
            
            // Block here until a connection is Accept()-ed and we can read.
            while (!_mutex.WaitOne(_mutextTimeout))
            {
                // Additional sleep here?
            }

            while (true)
            {
                // Receive will block until bytes are available to read.
                var received = _acceptedSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None, out var socketError);

                if (socketError != SocketError.Success)
                {
                    // Shit went wrong...
                    _acceptedSocket = null;
                    Task.Factory.StartNew(WaitForConnection);

                    // As we have the lock at this point we can release here and be done.
                    // This _must_ happen after we start the task for WaitFOrConnection
                    // otherwise that will bork because it acquires the lock too soon.
                    _mutex.ReleaseMutex();

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
            _socket.Close();
        }
    }
}

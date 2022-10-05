﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RoadCaptain.Adapters.Tests.Unit.Networking
{
    internal class NetworkConnection
    {
        private readonly int _port;
        private Socket? _listeningSocket;
        private readonly CancellationTokenSource _tokenSource;
        private readonly TimeSpan _acceptTimeout;
        private readonly TimeSpan _dataTimeout;
        private Socket? _clientSocket;

        public NetworkConnection(int port, int acceptTimeout, int dataTimeout)
        {
            _port = port;
            _tokenSource = new CancellationTokenSource();
            _acceptTimeout = TimeSpan.FromMilliseconds(acceptTimeout);
            _dataTimeout = TimeSpan.FromMilliseconds(dataTimeout);
        }

        public event EventHandler? AcceptTimeoutExpired;
        public event EventHandler? DataTimeoutExpired;
        public event EventHandler? ConnectionLost;
        public event EventHandler? ConnectionAccepted;
        public event EventHandler<DataEventArgs>? Data;

        public async Task StartAsync()
        {
            _listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true
            };

            _listeningSocket.Bind(new IPEndPoint(IPAddress.Loopback, _port));

            _listeningSocket.Listen();

            while (!_tokenSource.IsCancellationRequested)
            {
                var acceptTask = _listeningSocket.AcceptAsync(_tokenSource.Token).AsTask();

                while (!acceptTask.IsCompleted)
                {
                    var timeoutTask = Task.Delay(_acceptTimeout);

                    var completedTask = await Task.WhenAny(timeoutTask, acceptTask);

                    if (completedTask == timeoutTask)
                    {
                        AcceptTimeoutExpired?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        // When the network connection is stopped then
                        // the acceptTask potentially completes before
                        // the timeout task but it will be in a failed
                        // state (cancelled).
                        if (_tokenSource.IsCancellationRequested)
                        {
                            return;
                        }

                        _clientSocket = acceptTask.Result;

                        ConnectionAccepted?.Invoke(this, EventArgs.Empty);

                        break;
                    }
                }

                var dataReadTask = Task.Factory.StartNew(ReadDataFromClientSocket);

                while (!_tokenSource.IsCancellationRequested)
                {
                    var timeoutTask = Task.Delay(_dataTimeout);

                    var completedTask = await Task.WhenAny(timeoutTask, dataReadTask);

                    if (completedTask == timeoutTask)
                    {
                        DataTimeoutExpired?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        // When the network connection is stopped then
                        // the acceptTask potentially completes before
                        // the timeout task but it will be in a failed
                        // state (cancelled).
                        if (_tokenSource.IsCancellationRequested)
                        {
                            return;
                        }

                        if (dataReadTask.Result is { Length: > 0 })
                        {
                            Data?.Invoke(this, new DataEventArgs(dataReadTask.Result));
                        }

                        if (_clientSocket == null || !_clientSocket.Connected)
                        {
                            _clientSocket = null;

                            // Drop back to accepting a new connection
                            break;
                        }

                        if (_clientSocket.Connected)
                        {
                            dataReadTask = Task.Factory.StartNew(ReadDataFromClientSocket);
                        }
                    }
                }
            }
        }

        private byte[]? ReadDataFromClientSocket()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                var buffer = new byte[512];

                int read;

                try
                {
                    read = _clientSocket!.Receive(buffer, 0, buffer.Length, SocketFlags.None, out var socketError);

                    if (socketError != SocketError.Success || read == 0)
                    {
                        Debug.WriteLine("Socket error: " + socketError);

                        CloseAndCleanupClientSocket();

                        return null;
                    }
                }
                catch (SocketException)
                {
                    return null;
                }
                catch (ObjectDisposedException)
                {
                    return null;
                }

                if (read > 0)
                {
                    var result = new byte[read];
                    
                    for (var i = 0; i < read; i++)
                    {
                        result[i] = buffer[i];
                    }

                    return result;
                }
            }

            return null;
        }

        private void CloseAndCleanupClientSocket()
        {
            try
            {
                _clientSocket?.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException)
            {
                // Don't care
            }
            catch (ObjectDisposedException)
            {
                _clientSocket = null;
            }

            try
            {
                _clientSocket?.Close();
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
                _clientSocket = null;
            }
            
            ConnectionLost?.Invoke(this, EventArgs.Empty);
        }

        public void Stop()
        {
            _tokenSource.Cancel();

            try
            {
                _listeningSocket?.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException)
            {
                // Nop
            }

            try
            {
                _listeningSocket?.Close();
            }
            catch (SocketException)
            {
                // Nop
            }

            try
            {
                _listeningSocket?.Dispose();
            }
            catch
            {
                // Nop
            }
        }

    }

    public class DataEventArgs : EventArgs
    {
        public byte[] Data { get; }

        public DataEventArgs(byte[] data)
        {
            Data = data;
        }
    }
}
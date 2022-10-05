using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;

namespace RoadCaptain.Adapters.Tests.Unit.Networking
{
    public class WhenWaitingForIncomingConnection
    {
        private readonly NetworkConnection _networkConnection;
        private bool _noConnectionWatchdogRaised;
        private bool _connectionAcceptedRaised;
        private readonly int _port;
        private bool _noDataReceived;
        private bool _connectionLostRaised;
        private readonly List<byte> _receivedData = new();

        public WhenWaitingForIncomingConnection()
        {
            var random = new Random();
            _port = random.Next(1025, 10025);
            _networkConnection = new NetworkConnection(_port, 100, 100);
            _networkConnection.IncomingConnectionWatchdog += (_, _) => _noConnectionWatchdogRaised = true;
            _networkConnection.IncomingDataWatchdog += (_, _) => _noDataReceived = true;
            _networkConnection.Data += (_, args) => _receivedData.AddRange(args.Data);
            _networkConnection.ConnectionAccepted += (_, _) => _connectionAcceptedRaised = true;
            _networkConnection.ConnectionLost += (_, _) => _connectionLostRaised = true;
        }

        [Fact]
        public void GivenNoConnectionAfterTimeout_NoConnectionEventIsRaised()
        {
            var closeTask = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(250);
                _networkConnection.Stop();
            });

            try
            {
                _networkConnection.StartAsync().GetAwaiter().GetResult();
            }
            finally
            {
                closeTask.Wait();
            }

            _noConnectionWatchdogRaised.Should().BeTrue();
        }

        [Fact]
        public void GivenConnectionWithinTimeout_ConnectionAcceptedEventIsRaised()
        {
            // Setting this up may take some time
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var closeTask = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(250);
                _networkConnection.Stop();
            });

            try
            {
                var testTask = Task.Factory.StartNew(() =>
                {
                    // Need to give the network connection some time to start
                    // listening to the socket
                    Thread.Sleep(50);

                    clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                    Thread.Sleep(10);

                    clientSocket.Close();
                    clientSocket.Dispose();
                });

                var startTask = Task.Factory.StartNew(() => _networkConnection.StartAsync());

                Task.WhenAll(startTask, testTask).GetAwaiter().GetResult();
            }
            finally
            {
                closeTask.Wait();
            }

            _connectionAcceptedRaised.Should().BeTrue();
        }

        [Fact]
        public void GivenConnectionButNoDataWithinTimeout_NoDataEventIsRaised()
        {
            // Setting this up may take some time
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var closeTask = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(250);
                _networkConnection.Stop();
            });

            try
            {
                var testTask = Task.Factory.StartNew(() =>
                {
                    // Need to give the network connection some time to start
                    // listening to the socket
                    Thread.Sleep(50);

                    clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                    Thread.Sleep(150);

                    clientSocket.Close();
                    clientSocket.Dispose();
                });

                var startTask = Task.Factory.StartNew(() => _networkConnection.StartAsync());

                Task.WhenAll(startTask, testTask).GetAwaiter().GetResult();
            }
            finally
            {
                closeTask.Wait();
            }

            _noDataReceived.Should().BeTrue();
        }

        [Fact]
        public void GivenConnectionAndDataWithinTimeout_DataEventIsRaised()
        {
            // Setting this up may take some time
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var closeTask = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(250);
                _networkConnection.Stop();
            });

            try
            {
                var testTask = Task.Factory.StartNew(() =>
                {
                    // Need to give the network connection some time to start
                    // listening to the socket
                    Thread.Sleep(50);

                    clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));
                    
                    Thread.Sleep(10);

                    clientSocket.Send(new byte[] { 0x1, 0x2, 0x3 });
                    
                    Thread.Sleep(10);

                    clientSocket.Close();
                    clientSocket.Dispose();
                });

                var startTask = Task.Factory.StartNew(() => _networkConnection.StartAsync());

                Task.WhenAll(startTask, testTask).GetAwaiter().GetResult();
            }
            finally
            {
                closeTask.Wait();
            }

            _receivedData
                .Should()
                .BeEquivalentTo(new byte[] { 0x1, 0x2, 0x3 });
        }

        [Fact]
        public void GivenConnectionAndDataWithinTimeoutInMultipleSends_DataEventIsRaised()
        {
            // Setting this up may take some time
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var closeTask = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(250);
                _networkConnection.Stop();
            });

            try
            {
                var testTask = Task.Factory.StartNew(() =>
                {
                    // Need to give the network connection some time to start
                    // listening to the socket
                    Thread.Sleep(50);

                    clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));
                    
                    Thread.Sleep(10);

                    clientSocket.Send(new byte[] { 0x1, 0x2, 0x3 });
                    
                    Thread.Sleep(10);

                    clientSocket.Send(new byte[] { 0x4 });
                    
                    Thread.Sleep(10);

                    clientSocket.Send(new byte[] { 0x5, 0x6, 0x7 });
                    
                    Thread.Sleep(10);

                    clientSocket.Close();
                    clientSocket.Dispose();
                });

                var startTask = Task.Factory.StartNew(() => _networkConnection.StartAsync());

                Task.WhenAll(startTask, testTask).GetAwaiter().GetResult();
            }
            finally
            {
                closeTask.Wait();
            }

            _receivedData
                .Should()
                .BeEquivalentTo(new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7 });
        }

        [Fact]
        public void GivenConnectionAndDataThenClientClosesConnectionReconnectsAndSendsMoreData()
        {
            // Setting this up may take some time
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var secondClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var closeTask = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(250);
                if (!Debugger.IsAttached)
                {
                    _networkConnection.Stop();
                }
            });

            try
            {
                var testTask = Task.Factory.StartNew(() =>
                {
                    // Need to give the network connection some time to start
                    // listening to the socket
                    Thread.Sleep(50);

                    clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));
                    
                    Thread.Sleep(10);

                    var sent = clientSocket.Send(new byte[] { 0x1, 0x2, 0x3 });
                    
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();

                    Thread.Sleep(50);

                    secondClientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                    Thread.Sleep(10);

                    secondClientSocket.Send(new byte[] { 0x4, 0x5, 0x6 });
                    
                    secondClientSocket.Shutdown(SocketShutdown.Both);
                    secondClientSocket.Close();

                    clientSocket.Dispose();
                    secondClientSocket.Dispose();

                });

                var startTask = Task.Factory.StartNew(() => _networkConnection.StartAsync());

                Task.WhenAll(startTask, testTask).GetAwaiter().GetResult();
            }
            finally
            {
                closeTask.Wait();
            }

            _receivedData
                .Should()
                .BeEquivalentTo(new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 });
        }

        [Fact]
        public void GivenConnectionAndDataThenClientClosesConnection_ConnectionLostEventIsRaised()
        {
            // Setting this up may take some time
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var closeTask = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(250);
                if (!Debugger.IsAttached)
                {
                    _networkConnection.Stop();
                }
            });

            try
            {
                var testTask = Task.Factory.StartNew(() =>
                {
                    // Need to give the network connection some time to start
                    // listening to the socket
                    Thread.Sleep(50);

                    clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));
                    
                    Thread.Sleep(10);

                    var sent = clientSocket.Send(new byte[] { 0x1, 0x2, 0x3 });
                    
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    clientSocket.Dispose();

                });

                var startTask = Task.Factory.StartNew(() => _networkConnection.StartAsync());

                Task.WhenAll(startTask, testTask).GetAwaiter().GetResult();
            }
            finally
            {
                closeTask.Wait();
            }

            _connectionLostRaised.Should().BeTrue();
        }
    }
}

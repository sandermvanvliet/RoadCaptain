using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;

namespace RoadCaptain.Adapters.Tests.Unit.Networking
{
    public class WhenWaitingForIncomingConnection
    {
        private const int TestTimeoutMilliseconds = 250;
        private readonly NetworkConnection _networkConnection;
        private bool _noConnectionWatchdogRaised;
        private bool _connectionAcceptedRaised;
        private readonly int _port;
        private bool _noDataReceived;
        private bool _connectionLostRaised;
        private readonly List<byte> _receivedData = new();
        private readonly AutoResetEvent _autoResetEvent = new(false);

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
                _autoResetEvent.WaitOne(TestTimeoutMilliseconds);
                _networkConnection.Stop();
            });

            try
            {
                _networkConnection.StartAsync().GetAwaiter().GetResult();
            }
            finally
            {
                _autoResetEvent.Set();
                closeTask.Wait();
            }

            _noConnectionWatchdogRaised.Should().BeTrue();
        }

        [Fact]
        public void GivenConnectionWithinTimeout_ConnectionAcceptedEventIsRaised()
        {
            var clientSocket = GivenClientSocket();

            var closeTask = Task.Factory.StartNew(() =>
            {
                _autoResetEvent.WaitOne(TestTimeoutMilliseconds);
                _networkConnection.Stop();
            });

            try
            {
                var testTask = Task.Factory.StartNew(() =>
                {
                    WaitForConnectionToListen();

                    clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                    clientSocket.Close();
                    clientSocket.Dispose();

                    WaitForProcessingToHappen();
                });

                var startTask = Task.Factory.StartNew(() => _networkConnection.StartAsync());

                Task.WhenAll(startTask, testTask).GetAwaiter().GetResult();
            }
            finally
            {
                _autoResetEvent.Set();
                closeTask.Wait();
            }

            _connectionAcceptedRaised.Should().BeTrue();
        }

        [Fact]
        public void GivenConnectionButNoDataWithinTimeout_NoDataEventIsRaised()
        {
            var clientSocket = GivenClientSocket();

            var closeTask = Task.Factory.StartNew(() =>
            {
                _autoResetEvent.WaitOne(TestTimeoutMilliseconds);
                _networkConnection.Stop();
            });

            try
            {
                var testTask = Task.Factory.StartNew(() =>
                {
                    WaitForConnectionToListen();

                    clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                    // Receive timeout is 100ms so wait longer than that
                    Thread.Sleep(100);

                    clientSocket.Close();
                    clientSocket.Dispose();
                });

                var startTask = Task.Factory.StartNew(() => _networkConnection.StartAsync());

                Task.WhenAll(startTask, testTask).GetAwaiter().GetResult();
            }
            finally
            {
                _autoResetEvent.Set();
                closeTask.Wait();
            }

            _noDataReceived.Should().BeTrue();
        }

        [Fact]
        public void GivenConnectionAndDataWithinTimeout_DataEventIsRaised()
        {
            var clientSocket = GivenClientSocket();

            var closeTask = Task.Factory.StartNew(() =>
            {
                _autoResetEvent.WaitOne(TestTimeoutMilliseconds);
                _networkConnection.Stop();
            });

            try
            {
                var testTask = Task.Factory.StartNew(() =>
                {
                    WaitForConnectionToListen();

                    clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                    clientSocket.Send(new byte[] { 0x1, 0x2, 0x3 });

                    clientSocket.Close();
                    clientSocket.Dispose();

                    WaitForProcessingToHappen();
                });

                var startTask = Task.Factory.StartNew(() => _networkConnection.StartAsync());

                Task.WhenAll(startTask, testTask).GetAwaiter().GetResult();
            }
            finally
            {
                _autoResetEvent.Set();
                closeTask.Wait();
            }

            _receivedData
                .Should()
                .BeEquivalentTo(new byte[] { 0x1, 0x2, 0x3 });
        }

        [Fact]
        public void GivenConnectionAndDataWithinTimeoutInMultipleSends_DataEventIsRaised()
        {
            var clientSocket = GivenClientSocket();

            var closeTask = Task.Factory.StartNew(() =>
            {
                _autoResetEvent.WaitOne(TestTimeoutMilliseconds);
                _networkConnection.Stop();
            });

            try
            {
                var testTask = Task.Factory.StartNew(() =>
                {
                    WaitForConnectionToListen();

                    clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                    clientSocket.Send(new byte[] { 0x1, 0x2, 0x3 });

                    clientSocket.Send(new byte[] { 0x4 });

                    clientSocket.Send(new byte[] { 0x5, 0x6, 0x7 });

                    clientSocket.Close();
                    clientSocket.Dispose();

                    WaitForProcessingToHappen();
                });

                var startTask = Task.Factory.StartNew(() => _networkConnection.StartAsync());

                Task.WhenAll(startTask, testTask).GetAwaiter().GetResult();
            }
            finally
            {
                _autoResetEvent.Set();
                closeTask.Wait();
            }

            _receivedData
                .Should()
                .BeEquivalentTo(new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7 });
        }

        [Fact]
        public void GivenConnectionAndDataThenClientClosesConnectionReconnectsAndSendsMoreData()
        {
            var clientSocket = GivenClientSocket();
            var secondClientSocket = GivenClientSocket();

            var closeTask = Task.Factory.StartNew(() =>
            {
                _autoResetEvent.WaitOne(TestTimeoutMilliseconds);
                _networkConnection.Stop();
            });

            try
            {
                var testTask = Task.Factory.StartNew(() =>
                {
                    WaitForConnectionToListen();

                    clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                    clientSocket.Send(new byte[] { 0x1, 0x2, 0x3 });
                    
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();

                    WaitForProcessingToHappen();

                    secondClientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                    secondClientSocket.Send(new byte[] { 0x4, 0x5, 0x6 });
                    
                    secondClientSocket.Shutdown(SocketShutdown.Both);
                    secondClientSocket.Close();

                    clientSocket.Dispose();
                    secondClientSocket.Dispose();

                    WaitForProcessingToHappen();
                });

                var startTask = Task.Factory.StartNew(() => _networkConnection.StartAsync());

                Task.WhenAll(startTask, testTask).GetAwaiter().GetResult();
            }
            finally
            {
                _autoResetEvent.Set();
                closeTask.Wait();
            }

            _receivedData
                .Should()
                .BeEquivalentTo(new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 });
        }

        [Fact]
        public void GivenConnectionAndDataThenClientClosesConnection_ConnectionLostEventIsRaised()
        {
            var clientSocket = GivenClientSocket();

            var closeTask = Task.Factory.StartNew(() =>
            {
                _autoResetEvent.WaitOne(TestTimeoutMilliseconds);
                _networkConnection.Stop();
            });

            try
            {
                var testTask = Task.Factory.StartNew(() =>
                {
                    WaitForConnectionToListen();

                    clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));
                    
                    clientSocket.Send(new byte[] { 0x1, 0x2, 0x3 });
                    
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    clientSocket.Dispose();

                    WaitForProcessingToHappen();
                });

                var startTask = Task.Factory.StartNew(() => _networkConnection.StartAsync());

                Task.WhenAll(startTask, testTask).GetAwaiter().GetResult();
            }
            finally
            {
                _autoResetEvent.Set();
                closeTask.Wait();
            }

            _connectionLostRaised.Should().BeTrue();
        }

        private static Socket GivenClientSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private static void WaitForConnectionToListen()
        {
            // Need to give the network connection some time to start
            // listening to the socket
            Thread.Sleep(10);
        }

        private static void WaitForProcessingToHappen()
        {
            // Wait a short bit for the processing to happen
            Thread.Sleep(10);
        }
    }
}

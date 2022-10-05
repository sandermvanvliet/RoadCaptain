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
        private const int AcceptTimeoutMilliseconds = 100;
        private const int DataTimeoutMilliseconds = 100;
        private readonly AutoResetEvent _autoResetEvent = new(false);
        private readonly NetworkConnection _networkConnection;
        private readonly int _port;
        private readonly List<byte> _receivedData = new();
        private bool _connectionAcceptedRaised;
        private bool _connectionLostRaised;
        private bool _acceptTimeoutRaised;
        private bool _dataTimeoutRaised;

        public WhenWaitingForIncomingConnection()
        {
            var random = new Random();
            _port = random.Next(1025, 10025);
            _networkConnection = new NetworkConnection(_port, TimeSpan.FromMilliseconds(AcceptTimeoutMilliseconds), TimeSpan.FromMilliseconds(DataTimeoutMilliseconds));
            _networkConnection.AcceptTimeoutExpired += (_, _) => _acceptTimeoutRaised = true;
            _networkConnection.DataTimeoutExpired += (_, _) => _dataTimeoutRaised = true;
            _networkConnection.Data += (_, args) => _receivedData.AddRange(args.Data);
            _networkConnection.ConnectionAccepted += (_, _) => _connectionAcceptedRaised = true;
            _networkConnection.ConnectionLost += (_, _) => _connectionLostRaised = true;
        }

        [Fact]
        public void GivenNoConnectionAfterTimeout_AcceptTimeoutExpiredEventIsRaised()
        {
            WhenTestingConnection(_ =>
            {
                WaitForConnectionToListen();

                // Wait for the accept timeout to expire
                Thread.Sleep(AcceptTimeoutMilliseconds);
            });

            _acceptTimeoutRaised.Should().BeTrue();
        }

        [Fact]
        public void GivenConnectionWithinTimeout_ConnectionAcceptedEventIsRaised()
        {
            WhenTestingConnection(clientSocket =>
            {
                WaitForConnectionToListen();

                clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                WaitForProcessingToHappen();
            });

            _connectionAcceptedRaised.Should().BeTrue();
        }

        [Fact]
        public void GivenConnectionButNoDataWithinTimeout_DataTimeoutEventIsRaised()
        {
            WhenTestingConnection(clientSocket =>
            {
                WaitForConnectionToListen();

                clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                // Receive timeout is 100ms so wait longer than that
                Thread.Sleep(DataTimeoutMilliseconds + 20);
            });

            _dataTimeoutRaised.Should().BeTrue();
        }

        [Fact]
        public void GivenConnectionAndDataWithinTimeout_DataEventIsRaised()
        {
            WhenTestingConnection(clientSocket =>
            {
                WaitForConnectionToListen();

                clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                clientSocket.Send(new byte[] { 0x1, 0x2, 0x3 });

                WaitForProcessingToHappen();
            });

            _receivedData
                .Should()
                .BeEquivalentTo(new byte[] { 0x1, 0x2, 0x3 });
        }

        [Fact]
        public void GivenConnectionAndDataWithinTimeoutInMultipleSends_DataEventIsRaised()
        {
            WhenTestingConnection(clientSocket =>
            {
                WaitForConnectionToListen();

                clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                clientSocket.Send(new byte[] { 0x1, 0x2, 0x3 });

                clientSocket.Send(new byte[] { 0x4 });

                clientSocket.Send(new byte[] { 0x5, 0x6, 0x7 });

                WaitForProcessingToHappen();
            });

            _receivedData
                .Should()
                .BeEquivalentTo(new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7 });
        }

        [Fact]
        public void GivenConnectionAndDataThenClientClosesConnectionReconnectsAndSendsMoreData()
        {
            WhenTestingConnection((clientSocket, secondClientSocket) =>
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

                WaitForProcessingToHappen();
            });

            _receivedData
                .Should()
                .BeEquivalentTo(new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 });
        }

        [Fact]
        public void GivenConnectionAndDataThenClientClosesConnection_ConnectionLostEventIsRaised()
        {
            WhenTestingConnection(clientSocket =>
            {
                WaitForConnectionToListen();

                clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                clientSocket.Send(new byte[] { 0x1, 0x2, 0x3 });

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();

                WaitForProcessingToHappen();
            });

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

        private void WhenTestingConnection(Action<Socket> when)
        {
            var clientSocket = GivenClientSocket();

            var closeTask = Task.Factory.StartNew(() =>
            {
                _autoResetEvent.WaitOne(TestTimeoutMilliseconds);
                _networkConnection.Shutdown();
            });

            try
            {
                // ReSharper disable AccessToDisposedClosure
                var testTask = Task.Factory.StartNew(() => { when(clientSocket); });
                // ReSharper restore AccessToDisposedClosure

                var startTask = Task.Factory.StartNew(() => _networkConnection.StartAsync());

                Task.WhenAll(startTask, testTask).GetAwaiter().GetResult();
            }
            finally
            {
                _autoResetEvent.Set();
                closeTask.Wait();
            }

            clientSocket.Dispose();
        }

        private void WhenTestingConnection(Action<Socket, Socket> testAction)
        {
            var clientSocket = GivenClientSocket();
            var secondClientSocket = GivenClientSocket();

            var closeTask = Task.Factory.StartNew(() =>
            {
                _autoResetEvent.WaitOne(TestTimeoutMilliseconds);
                _networkConnection.Shutdown();
            });

            try
            {
                // ReSharper disable AccessToDisposedClosure
                var testTask = Task.Factory.StartNew(() => { testAction(clientSocket, secondClientSocket); });
                // ReSharper restore AccessToDisposedClosure

                var startTask = Task.Factory.StartNew(() => _networkConnection.StartAsync());

                Task.WhenAll(startTask, testTask).GetAwaiter().GetResult();
            }
            finally
            {
                _autoResetEvent.Set();
                closeTask.Wait();
            }

            clientSocket.Dispose();
            secondClientSocket.Dispose();
        }
    }
}
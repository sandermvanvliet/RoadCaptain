using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RoadCaptain.GameStates;

namespace RoadCaptain.Adapters.Tests.Unit.Networking
{
    public class WhenWaitingForIncomingConnection
    {
        private const int TestTimeoutMilliseconds = 250;
        private const int AcceptTimeoutMilliseconds = 100;
        private const int DataTimeoutMilliseconds = 100;
        private readonly AutoResetEvent _autoResetEvent = new(false);
        private readonly NetworkConnection _networkConnection;
        private readonly InMemoryGameStateDispatcher _gameStateDispatcher;
        private readonly int _port;
        private readonly List<byte> _receivedData = new();
        private readonly List<GameState> _receivedGameStates = new();
        private bool _connectionAcceptedRaised;
        private bool _connectionLostRaised;
        private bool _acceptTimeoutRaised;
        private bool _dataTimeoutRaised;

        public WhenWaitingForIncomingConnection()
        {
            var random = new Random();
            _port = random.Next(1025, 10025);

            var monitoringEvents = new NopMonitoringEvents();

            _gameStateDispatcher = new InMemoryGameStateDispatcher(monitoringEvents);
            _gameStateDispatcher.LoggedIn();
            _gameStateDispatcher.Dispatch(new ReadyToGoState());
            _gameStateDispatcher.ReceiveGameState(gameState => _receivedGameStates.Add(gameState));

            _networkConnection = new NetworkConnection(
                _port, 
                TimeSpan.FromMilliseconds(AcceptTimeoutMilliseconds), 
                TimeSpan.FromMilliseconds(DataTimeoutMilliseconds),
                16,
                _gameStateDispatcher,
                monitoringEvents,
                null);

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
        public void GivenNetworkConnectionStartsAndListens_WaitingForConnectionStateIsDispatched()
        {
            using var tokenSource = new CancellationTokenSource();
            
            Task.Factory.StartNew(() => _gameStateDispatcher.Start(tokenSource.Token), tokenSource.Token);

            WhenTestingConnection(
                _ =>
                {
                    WaitForConnectionToListen();

                    WaitForProcessingToHappen();
                });

            _gameStateDispatcher.Drain();

            tokenSource.Cancel();

            _receivedGameStates
                .Should()
                .Contain(gameState => gameState is WaitingForConnectionState);
        }

        [Fact]
        public void GivenConnectionAccepted_ConnectedToZwiftStateIsDispatched()
        {
            using var tokenSource = new CancellationTokenSource();
            
            Task.Factory.StartNew(() => _gameStateDispatcher.Start(tokenSource.Token), tokenSource.Token);

            WhenTestingConnection(
                clientSocket =>
                {
                    WaitForConnectionToListen();

                    clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                    WaitForProcessingToHappen();
                });

            _gameStateDispatcher.Drain();

            tokenSource.Cancel();

            _receivedGameStates
                .Should()
                .Contain(gameState => gameState is ConnectedToZwiftState);
        }

        [Fact]
        public void GivenConnectedClientDisconnects_WaitingForConnectionStateIsDispatched()
        {
            using var tokenSource = new CancellationTokenSource();
            
            Task.Factory.StartNew(() => _gameStateDispatcher.Start(tokenSource.Token), tokenSource.Token);

            WhenTestingConnection(
                clientSocket =>
                {
                    WaitForConnectionToListen();

                    clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                    WaitForProcessingToHappen();

                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Disconnect(false);

                    WaitForProcessingToHappen();
                });

            _gameStateDispatcher.Drain();

            tokenSource.Cancel();

            _receivedGameStates
                .OfType<WaitingForConnectionState>()
                .Should()
                .HaveCount(2);
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
        public void GivenConnectionAndDataSentInOneGoIsLargerThanReceiveBuffer_AllDataIsReceived()
        {
            var data = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF, 0x10, 0x11, 0x12, 0x13, 0x14 };
            WhenTestingConnection(clientSocket =>
            {
                WaitForConnectionToListen();

                clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                clientSocket.Send(data);

                WaitForProcessingToHappen();
            });

            _receivedData
                .Should()
                .BeEquivalentTo(data);
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

        [Fact]
        public void GivenConnectedClientButNoData_ReceiveMessageBytesBlocks()
        {
            var receiveTask = Task.Factory.StartNew(() => _networkConnection.ReceiveMessageBytes());

            WhenTestingConnection(clientSocket =>
            {
                WaitForConnectionToListen();

                clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));
                
                // Don't send new data
                Thread.Sleep(DataTimeoutMilliseconds + 25);
            });

            receiveTask
                .Status
                .Should()
                .Be(TaskStatus.Running);
        }

        [Fact]
        public void GivenNoConnectedClient_ReceiveMessageBytesBlocks()
        {
            var receiveTask = Task.Factory.StartNew(() => _networkConnection.ReceiveMessageBytes());

            WhenTestingConnection(_ =>
            {
                WaitForConnectionToListen();

                // Don't connect
                Thread.Sleep(DataTimeoutMilliseconds + 25);
            });

            receiveTask
                .Status
                .Should()
                .Be(TaskStatus.Running);
        }

        [Fact]
        public void GivenConnectedClientAndDataIsSent_ReceiveMessageBytesReturnsData()
        {
            var receiveTask = Task.Factory.StartNew(() => _networkConnection.ReceiveMessageBytes());

            WhenTestingConnection(clientSocket =>
            {
                WaitForConnectionToListen();

                clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));

                clientSocket.Send(new byte[] { 0x1, 0x2, 0x3 });
                
                WaitForProcessingToHappen();

                receiveTask
                    .GetAwaiter()
                    .GetResult()
                    .Should()
                    .BeEquivalentTo(new byte[] { 0x1, 0x2, 0x3 });
            });
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
                if (!Debugger.IsAttached)
                {
                    _networkConnection.Shutdown();
                }
            });

            try
            {
                // ReSharper disable AccessToDisposedClosure
                var testTask = Task.Factory.StartNew(() => { when(clientSocket); });
                // ReSharper restore AccessToDisposedClosure

                _networkConnection.StartAsync().GetAwaiter().GetResult();

                testTask.GetAwaiter().GetResult();
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
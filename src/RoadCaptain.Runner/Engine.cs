using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.Commands;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;
using Serilog;

namespace RoadCaptain.Runner
{
    internal class Engine
    {
        private readonly Configuration _configuration;
        private readonly ConnectToZwiftUseCase _connectUseCase;
        private readonly IGameStateReceiver _gameStateReceiver;
        private readonly HandleZwiftMessagesUseCase _handleMessageUseCase;
        private readonly DecodeIncomingMessagesUseCase _listenerUseCase;
        private readonly LoadRouteUseCase _loadRouteUseCase;
        private readonly ILogger _logger;
        private readonly NavigationUseCase _navigationUseCase;
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly IWindowService _windowService;
        private CancellationTokenSource _connectionToken = new();
        private Task _gameStateReceiverTask;
        private Task _initiatorTask;
        private Task _listenerTask;
        private Task _messageHandlingTask;
        private CancellationTokenSource _messageHandlingToken;
        private Task _navigationTask;
        private CancellationTokenSource _navigationToken;
        private GameState _previousGameState;

        public Engine(
            ILogger logger,
            LoadRouteUseCase loadRouteUseCase,
            Configuration configuration,
            IWindowService windowService,
            DecodeIncomingMessagesUseCase listenerUseCase,
            ConnectToZwiftUseCase connectUseCase,
            HandleZwiftMessagesUseCase handleMessageUseCase,
            NavigationUseCase navigationUseCase, 
            IGameStateReceiver gameStateReceiver)
        {
            _logger = logger;
            _loadRouteUseCase = loadRouteUseCase;
            _configuration = configuration;
            _windowService = windowService;
            _listenerUseCase = listenerUseCase;
            _connectUseCase = connectUseCase;
            _handleMessageUseCase = handleMessageUseCase;
            _navigationUseCase = navigationUseCase;
            _gameStateReceiver = gameStateReceiver;
        }

        private void GameStateReceived(GameState gameState)
        {
            _logger.Debug(
                "Transitioning from '{CurrentGameState}' to '{NewGameState}'",
                _previousGameState?.GetType().Name ?? "initial",
                gameState.GetType().Name);

            if (gameState is LoggedInState)
            {
                _logger.Information("User logged in");

                // Once the user has logged in we need to do two things:
                // 1. Start the connection listener (DecodeIncomingMessagesUseCase)
                // 2. Start the connection initiator (ConnectToZwiftUseCase)
                // When the listener picks up a new connection it will
                // dispatch the ConnectedToZwift state.
                _connectionToken = new CancellationTokenSource();

                StartZwiftConnectionListener();
                StartZwiftConnectionInitiator();
            }
            else if (gameState is NotLoggedInState)
            {
                // Stop the connection initiator and listener
                CancelAndCleanUp(_connectionToken, () => _listenerTask);
                CancelAndCleanUp(_connectionToken, () => _initiatorTask);

                if (_messageHandlingTask.IsRunning())
                {
                    CancelAndCleanUp(_messageHandlingToken, () => _messageHandlingTask);
                }
            }
            else if (gameState is WaitingForConnectionState)
            {
                _logger.Information("Waiting for connection from Zwift");
            }
            else if (gameState is ConnectedToZwiftState)
            {
                _logger.Information("Connected to Zwift");

                _loadRouteUseCase.Execute(new LoadRouteCommand { Path = _configuration.Route });

                // Start handling Zwift messages
                StartMessageHandler();
            }

            if (gameState is InGameState && _previousGameState is not InGameState)
            {
                _logger.Information("User entered the game");

                // Start navigation if it is not running
                if (!_navigationTask.IsRunning())
                {
                    StartNavigation();
                }
            }

            if (gameState is ErrorState errorState)
            {
                _windowService.ShowErrorDialog(errorState.Exception.Message);
            }

            _previousGameState = gameState;
        }

        private void CancelAndCleanUp(CancellationTokenSource tokenSource, Expression<Func<Task>> func)
        {
            if (tokenSource is { IsCancellationRequested: false })
            {
                tokenSource.Cancel();
            }

            if (func.Body is MemberExpression { Member: FieldInfo fieldInfo })
            {
                if (fieldInfo.GetValue(this) is Task task)
                {
                    task.SafeWaitForCancellation();

                    fieldInfo.SetValue(this, null);
                }
            }
        }

        private void StartZwiftConnectionListener()
        {
            if (_listenerTask.IsRunning())
            {
                return;
            }

            _logger.Information("Starting connection listener");

            _listenerTask = Task.Factory.StartNew(() => _listenerUseCase.ExecuteAsync(_connectionToken.Token),
                TaskCreationOptions.LongRunning);
        }

        private void StartZwiftConnectionInitiator()
        {
            if (_initiatorTask.IsRunning())
            {
                return;
            }

            _logger.Information("Starting connection initiator");

            _initiatorTask = Task.Factory.StartNew(() =>
                    _connectUseCase
                        .ExecuteAsync(
                            new ConnectCommand { AccessToken = _configuration.AccessToken },
                            _connectionToken.Token),
                TaskCreationOptions.LongRunning);
        }

        private void StartMessageHandler()
        {
            _logger.Information("Starting message handler");

            _messageHandlingToken = new CancellationTokenSource();
            _messageHandlingTask = Task.Factory.StartNew(
                () => _handleMessageUseCase.Execute(_messageHandlingToken.Token),
                TaskCreationOptions.LongRunning);
        }

        private void StartNavigation()
        {
            _logger.Information("Starting navigation");

            _navigationToken = new CancellationTokenSource();
            _navigationTask = Task.Factory.StartNew(
                () => _navigationUseCase.Execute(_navigationToken.Token),
                TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            CancelAndCleanUp(_tokenSource, () => _gameStateReceiverTask);
        }

        public void Start()
        {
            _gameStateReceiver.Register(null, null, GameStateReceived);
            _gameStateReceiverTask = Task.Factory.StartNew(
                () => _gameStateReceiver.Start(_tokenSource.Token),
                TaskCreationOptions.LongRunning);
        }
    }
}
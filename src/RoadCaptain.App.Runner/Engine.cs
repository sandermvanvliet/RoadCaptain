using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.UserPreferences;
using RoadCaptain.Commands;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;

namespace RoadCaptain.App.Runner
{
    public class Engine
    {
        private readonly Configuration _configuration;
        private readonly ConnectToZwiftUseCase _connectUseCase;
        private readonly IGameStateReceiver _gameStateReceiver;
        private readonly HandleZwiftMessagesUseCase _handleMessageUseCase;
        private readonly DecodeIncomingMessagesUseCase _listenerUseCase;
        private readonly LoadRouteUseCase _loadRouteUseCase;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly NavigationUseCase _navigationUseCase;
        private readonly ISegmentStore _segmentStore;
        private readonly IWindowService _windowService;

        private TaskWithCancellation? _gameStateReceiverTask;
        private TaskWithCancellation? _initiatorTask;
        private TaskWithCancellation? _listenerTask;
        private PlannedRoute? _loadedRoute;
        private TaskWithCancellation? _messageHandlingTask;
        private TaskWithCancellation? _navigationTask;

        private GameState? _previousGameState;
        private readonly IZwiftGameConnection _zwiftGameConnection;
        private ulong _lastSequenceNumber;
        private readonly IUserPreferences _userPreferences;
        private readonly string _connectionEncryptionSecret;

        public Engine(
            MonitoringEvents monitoringEvents,
            LoadRouteUseCase loadRouteUseCase,
            Configuration configuration,
            IWindowService windowService,
            DecodeIncomingMessagesUseCase listenerUseCase,
            ConnectToZwiftUseCase connectUseCase,
            HandleZwiftMessagesUseCase handleMessageUseCase,
            NavigationUseCase navigationUseCase,
            IGameStateReceiver gameStateReceiver, 
            ISegmentStore segmentStore, 
            IZwiftGameConnection zwiftGameConnection, IUserPreferences userPreferences)
        {
            _monitoringEvents = monitoringEvents;
            _loadRouteUseCase = loadRouteUseCase;
            _configuration = configuration;
            _windowService = windowService;
            _listenerUseCase = listenerUseCase;
            _connectUseCase = connectUseCase;
            _handleMessageUseCase = handleMessageUseCase;
            _navigationUseCase = navigationUseCase;
            _gameStateReceiver = gameStateReceiver;
            _segmentStore = segmentStore;
            _zwiftGameConnection = zwiftGameConnection;
            _userPreferences = userPreferences;

            _gameStateReceiver.Register(
                route => _loadedRoute = route,
                sequenceNumber => _lastSequenceNumber = sequenceNumber,
                GameStateReceived);

            _connectionEncryptionSecret = GenerateSecret();
        }

        private static string GenerateSecret()
        {
            // This is an AES 128 bit key
            var aes = Aes.Create();
            aes.KeySize = 128;
            aes.GenerateKey();
            return Convert.ToBase64String(aes.Key);
        }

        protected void GameStateReceived(GameState gameState)
        {
            // Only log actual transitions
            if (_previousGameState != null && _previousGameState.GetType() != gameState.GetType())
            {
                _monitoringEvents.StateTransition(_previousGameState, gameState);
            }

            if (gameState is LoggedInState loggedInState)
            {
                _monitoringEvents.Information("User logged in");

                // Once the user has logged in we need to do two things:
                // 1. Start the connection listener (DecodeIncomingMessagesUseCase)
                // 2. Start the connection initiator (ConnectToZwiftUseCase)
                // When the listener picks up a new connection it will
                // dispatch the ConnectedToZwift state.
                StartZwiftConnectionListener();
                StartZwiftConnectionInitiator(loggedInState.AccessToken);
            }
            else if (gameState is NotLoggedInState)
            {
                // Stop the connection initiator and listener
                CancelAndCleanUp(() => _listenerTask);
                CancelAndCleanUp(() => _initiatorTask);

                if (_messageHandlingTask.IsRunning())
                {
                    CancelAndCleanUp(() => _messageHandlingTask);
                }
            }
            else if (gameState is WaitingForConnectionState)
            {
                _monitoringEvents.Information("Waiting for connection from Zwift");

                if (_loadedRoute != null)
                {
                    _windowService.ShowInGameWindow(CreateInGameViewModel(_loadedRoute));
                }
            }
            else if (gameState is ConnectedToZwiftState && _previousGameState is InGameState)
            {
                _monitoringEvents.Information("User left activity");

                StartMessageHandler();

                _windowService.ShowMainWindow();
            }
            else if (gameState is ConnectedToZwiftState)
            {
                _monitoringEvents.Information("Connected to Zwift");

                if (!string.IsNullOrEmpty(_configuration.Route))
                {
                    _loadRouteUseCase.Execute(new LoadRouteCommand { Path = _configuration.Route });
                }

                // Start handling Zwift messages
                StartMessageHandler();
            }
            else if (gameState is InvalidCredentialsState invalidCredentials)
            {
                // Stop the connection initiator and listener
                CancelAndCleanUp(() => _listenerTask);
                CancelAndCleanUp(() => _initiatorTask);
                CancelAndCleanUp(() => _navigationTask);
                CancelAndCleanUp(() => _messageHandlingTask);

                // Clear token info on main window view model
                // and show main window
                _windowService.ShowErrorDialog(invalidCredentials.Exception.Message);
                _windowService.ShowMainWindow();
            }

            if (gameState is InGameState && _previousGameState is not InGameState)
            {
                _monitoringEvents.Information("User entered the game");

                // Start navigation if it is not running
                StartNavigation();
            }

            if (gameState is CompletedRouteState completed)
            {
                if (_userPreferences.EndActivityAtEndOfRoute)
                {
                    _zwiftGameConnection.EndActivity(_lastSequenceNumber, "RoadCaptain: " + completed.Route.Name, completed.RiderId);
                }
                else if (completed.Route.IsLoop && _userPreferences.LoopRouteAtEndOfRoute)
                {
                    completed.Route.EnterNextLoop();
                }
            }

            if (gameState is ErrorState errorState)
            {
                _windowService.ShowErrorDialog(errorState.Exception.Message);
            }

            _previousGameState = gameState;
        }

        private InGameNavigationWindowViewModel CreateInGameViewModel(PlannedRoute plannedRoute)
        {
            var segments = _segmentStore.LoadSegments(plannedRoute.World, plannedRoute.Sport);

            var inGameWindowModel = new InGameWindowModel(segments)
            {
                Route = plannedRoute
            };

            var viewModel = new InGameNavigationWindowViewModel(inGameWindowModel, segments, _zwiftGameConnection);

            if (_previousGameState != null)
            {
                viewModel.UpdateGameState(_previousGameState);
            }

            return viewModel;
        }

        private void StartZwiftConnectionListener()
        {
            if (_listenerTask.IsRunning())
            {
                return;
            }

            _monitoringEvents.Information("Starting connection listener");

            _listenerTask =
                TaskWithCancellation.Start(cancellationToken => _listenerUseCase.ExecuteAsync(_connectionEncryptionSecret, cancellationToken));
        }

        private void StartZwiftConnectionInitiator(string accessToken)
        {
            if (_initiatorTask.IsRunning())
            {
                return;
            }

            _monitoringEvents.Information("Starting connection initiator");

#pragma warning disable CS8602 // This method is always called after StartZwiftConnectionListener()
            _initiatorTask = _listenerTask.StartLinkedTask(
#pragma warning restore CS8602
                token => _connectUseCase
                    .ExecuteAsync(
                        new ConnectCommand { AccessToken = accessToken, ConnectionEncryptionSecret = _connectionEncryptionSecret },
                        token)
                    .GetAwaiter()
                    .GetResult());
        }

        private void StartMessageHandler()
        {
            if (_messageHandlingTask.IsRunning())
            {
                return;
            }

            _monitoringEvents.Information("Starting message handler");

            _messageHandlingTask = TaskWithCancellation.Start(token => _handleMessageUseCase.Execute(token));
        }

        private void StartNavigation()
        {
            if (_navigationTask.IsRunning())
            {
                return;
            }

            _monitoringEvents.Information("Starting navigation");

            _navigationTask = TaskWithCancellation.Start(token => _navigationUseCase.Execute(token));
        }

        private void CancelAndCleanUp(Expression<Func<TaskWithCancellation?>> func)
        {
            if (func.Body is MemberExpression { Member: FieldInfo fieldInfo })
            {
                if (fieldInfo.GetValue(this) is TaskWithCancellation task)
                {
                    try
                    {
                        task.Cancel();
                    }
                    catch (Exception e)
                    {
                        _monitoringEvents.Error(e,
                            $"Cleaning up task {fieldInfo.Name.Replace("_", "").Replace("Task", "")} failed");
                    }

                    fieldInfo.SetValue(this, null);
                }
            }
        }

        public void Stop()
        {
            CancelAndCleanUp(() => _gameStateReceiverTask);
            CancelAndCleanUp(() => _messageHandlingTask);
            CancelAndCleanUp(() => _listenerTask);
            CancelAndCleanUp(() => _initiatorTask);
            CancelAndCleanUp(() => _navigationTask);
        }

        public void Start()
        {
            _gameStateReceiverTask = TaskWithCancellation.Start(token => _gameStateReceiver.Start(token));
        }
    }
}
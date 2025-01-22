// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.App.Runner.Models;
using RoadCaptain.App.Runner.ViewModels;
using RoadCaptain.App.Shared;
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
        private readonly IZwiftCredentialCache _credentialCache;
        private DateTime _connectionWatchdogTimestamp;
        private DateTime _dataWatchdogTimestamp;
        private readonly TimeSpan _watchdogTimeout = TimeSpan.FromSeconds(15);
        private readonly Timer _watchdogTimer;

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
            IZwiftGameConnection zwiftGameConnection,
            IUserPreferences userPreferences,
            IZwiftCredentialCache credentialCache)
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
            _credentialCache = credentialCache;

            _gameStateReceiver.ReceiveRoute(route =>
                {
                    _loadedRoute = route;
                    _monitoringEvents.RouteLoaded(route);
                });
            _gameStateReceiver.ReceiveLastSequenceNumber(sequenceNumber => _lastSequenceNumber = sequenceNumber);
            _gameStateReceiver.ReceiveGameState(GameStateReceived);

            _watchdogTimer = new Timer(_ =>
            {
                Watchdog()
                    .GetAwaiter()
                    .GetResult();
            });
        }

        protected void GameStateReceived(GameState gameState)
        {
            _dataWatchdogTimestamp = DateTime.UtcNow;

            // Only log actual transitions
            if (_previousGameState != null && _previousGameState.GetType() != gameState.GetType())
            {
                _monitoringEvents.StateTransition(_previousGameState, gameState);
            }

            if (gameState is LoggedInState)
            {
                _monitoringEvents.Information("User logged in");
            }
            else if (gameState is ReadyToGoState)
            {
                _monitoringEvents.Information("User is ready to go and start the route");

                // Only hit this branch when we are not yet connected.
                // This situation happens when the user ends an activity
                // and then wants to start another route.
                if (_previousGameState is not ConnectedToZwiftState)
                {
                    var credentials = _credentialCache.LoadAsync().GetAwaiter().GetResult();
                    if (credentials == null || string.IsNullOrEmpty(credentials.AccessToken))
                    {
                        _monitoringEvents.Error("No Zwift credentials available, cannot initiate Zwift connection");
                    }
                    else
                    {
                        StartZwiftConnectionInitiator(credentials.AccessToken);

                        // Once the user has logged in we need to do two things:
                        // 1. Start the connection listener (DecodeIncomingMessagesUseCase)
                        // 2. Start the connection initiator (ConnectToZwiftUseCase)
                        // When the listener picks up a new connection it will
                        // dispatch the ConnectedToZwift state.
                        StartZwiftConnectionListener();
                    }
                }

                if (_loadedRoute != null)
                {
                    _windowService.ShowInGameWindow(CreateInGameViewModel(_loadedRoute, gameState));
                }
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

                // Reset the watchdog timer for new connections
                _connectionWatchdogTimestamp = DateTime.UtcNow;

                if (_loadedRoute != null)
                {
                    _windowService.ShowInGameWindow(CreateInGameViewModel(_loadedRoute, gameState));
                }
            }
            else if (gameState is ConnectedToZwiftState && GameState.IsInGame(_previousGameState))
            {
                _monitoringEvents.Information("User left activity");

                StartMessageHandler();

                _windowService.ShowMainWindow();
            }
            else if (gameState is ConnectedToZwiftState && _previousGameState is WaitingForConnectionState)
            {
                _monitoringEvents.Information("Connected to Zwift");

                // Reset the watchdog timer for new data (== new game states)
                _dataWatchdogTimestamp = DateTime.UtcNow;

                if (_loadedRoute == null && !string.IsNullOrEmpty(_configuration.Route))
                {
                    _loadRouteUseCase.Execute(new LoadRouteCommand { Path = _configuration.Route });
                }

                // Start handling Zwift messages
                StartMessageHandler();

                if (_loadedRoute != null)
                {
                    _windowService.ShowInGameWindow(CreateInGameViewModel(_loadedRoute, gameState));
                }
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

                _windowService.ShowMainWindow();
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

            if (GameState.IsInGame(gameState) && !GameState.IsInGame(_previousGameState))
            {
                _monitoringEvents.Information("User entered the game");

                // Start navigation if it is not running
                StartNavigation();
            }

            if (GameState.IsInGame(gameState) && _loadedRoute != null && !GameState.IsInGame(_previousGameState))
            {
                _windowService.ShowInGameWindow(CreateInGameViewModel(_loadedRoute, gameState));
            }

            if (gameState is CompletedRouteState completed)
            {
                if (_userPreferences.EndActivityAtEndOfRoute)
                {
                    _zwiftGameConnection.EndActivity(_lastSequenceNumber, "RoadCaptain: " + completed.Route.Name, completed.RiderId);
                }
            }

            if (gameState is ErrorState errorState)
            {
                _windowService.ShowErrorDialog(errorState.Exception?.Message ?? errorState.Message);
            }

            if (gameState is IncorrectConnectionSecretState && _previousGameState is not IncorrectConnectionSecretState)
            {
                _monitoringEvents.Information("Connection secret got out of whack, initiating relay again");

                var credentials = _credentialCache.LoadAsync().GetAwaiter().GetResult();

                if (credentials == null || string.IsNullOrEmpty(credentials.AccessToken))
                {
                    _monitoringEvents.Error("No Zwift credentials available, cannot initiate Zwift connection");
                }
                else
                {
                    CancelAndCleanUp(() => _listenerTask);

                    StartZwiftConnectionInitiator(credentials.AccessToken);

                    // Once the user has logged in we need to do two things:
                    // 1. Start the connection listener (DecodeIncomingMessagesUseCase)
                    // 2. Start the connection initiator (ConnectToZwiftUseCase)
                    // When the listener picks up a new connection it will
                    // dispatch the ConnectedToZwift state.
                    StartZwiftConnectionListener();
                }
            }

            _previousGameState = gameState;
        }

        private InGameNavigationWindowViewModel CreateInGameViewModel(PlannedRoute plannedRoute, GameState gameState)
        {
            if (plannedRoute.World == null)
            {
                throw new Exception("Route does not have a world set");
            }

            var segments = _segmentStore.LoadSegments(plannedRoute.World, plannedRoute.Sport);

            var inGameWindowModel = new InGameWindowModel(segments)
            {
                Route = plannedRoute
            };

            var viewModel = new InGameNavigationWindowViewModel(inGameWindowModel, segments, _zwiftGameConnection, _monitoringEvents, _windowService);

            viewModel.UpdateGameState(gameState);

            return viewModel;
        }

        private void StartZwiftConnectionListener()
        {
            if (_listenerTask.IsRunning())
            {
                return;
            }

            _monitoringEvents.Information("Starting connection listener");

            if (_initiatorTask != null)
            {
                _listenerTask = _initiatorTask
                    .StartLinkedTask(
                        async cancellationToken =>
                        {
                            await Task.Delay(2000, cancellationToken);
                            await _listenerUseCase.ExecuteAsync(cancellationToken);
                        });
            }
            else
            {
                _listenerTask =
                    TaskWithCancellation.Start(
                        async cancellationToken =>
                        {
                            await Task.Delay(2000, cancellationToken);
                            await _listenerUseCase.ExecuteAsync(cancellationToken);
                        });
            }
        }

        private void StartZwiftConnectionInitiator(string accessToken)
        {
            if (_initiatorTask.IsRunning())
            {
                return;
            }

            _monitoringEvents.Information("Starting connection initiator");

            if (_listenerTask != null)
            {
                _initiatorTask = _listenerTask.StartLinkedTask(
                    async _ => await _connectUseCase
                        .ExecuteAsync(
                            new ConnectCommand
                            {
                                AccessToken = accessToken,
                                ConnectionEncryptionSecret = _userPreferences.ConnectionSecret
                            }));
            }
            else
            {
                _initiatorTask = TaskWithCancellation.Start(
                    async _ => await _connectUseCase
                        .ExecuteAsync(
                            new ConnectCommand
                            {
                                AccessToken = accessToken,
                                ConnectionEncryptionSecret = _userPreferences.ConnectionSecret
                            }));
            }
        }

        private void StartMessageHandler()
        {
            if (_messageHandlingTask.IsRunning())
            {
                return;
            }

            _monitoringEvents.Information("Starting message handler");

            _messageHandlingTask = TaskWithCancellation.Start(cancellationToken =>
            {
                _handleMessageUseCase.Execute(cancellationToken);
                return Task.CompletedTask;
            });
        }

        private void StartNavigation()
        {
            if (_navigationTask.IsRunning())
            {
                return;
            }

            _monitoringEvents.Information("Starting navigation");

            _navigationTask = TaskWithCancellation.Start(cancellationToken =>
            {
                _navigationUseCase.Execute(cancellationToken);
                return Task.CompletedTask;
            });
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

        public void Start()
        {
            _gameStateReceiverTask = TaskWithCancellation.Start(cancellationToken =>
            {
                _gameStateReceiver.Start(cancellationToken);
                return Task.CompletedTask;
            });

            _watchdogTimer.Change(_watchdogTimeout, _watchdogTimeout);
        }

        public void Stop()
        {
            _watchdogTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            CancelAndCleanUp(() => _gameStateReceiverTask);
            CancelAndCleanUp(() => _messageHandlingTask);
            CancelAndCleanUp(() => _listenerTask);
            CancelAndCleanUp(() => _initiatorTask);
            CancelAndCleanUp(() => _navigationTask);
        }


        private async Task Watchdog()
        {
            await ConnectionWatchdog();
            await DataWatchdog();
        }

        public async Task ConnectionWatchdog()
        {
            if (_previousGameState is WaitingForConnectionState)
            {
                if (DateTime.UtcNow.Subtract(_connectionWatchdogTimestamp) > _watchdogTimeout)
                {
                    _monitoringEvents.Warning(
                        "No connection was made by Zwift since {Timestamp}, re-initiating the connection",
                        _connectionWatchdogTimestamp);
                    _connectionWatchdogTimestamp = DateTime.UtcNow;

                    var credentials = _credentialCache.LoadAsync().GetAwaiter().GetResult();
                    if (credentials == null || string.IsNullOrEmpty(credentials.AccessToken))
                    {
                        _monitoringEvents.Error("No Zwift credentials available, cannot initiate Zwift connection");
                        return;
                    }

                    await _connectUseCase
                        .ExecuteAsync(new ConnectCommand
                        {
                            ConnectionEncryptionSecret = _userPreferences.ConnectionSecret,
                            AccessToken = credentials.AccessToken!
                        });
                }
            }
        }

        private async Task DataWatchdog()
        {
            if (GameState.IsInGame(_previousGameState))
            {
                if (DateTime.UtcNow.Subtract(_dataWatchdogTimestamp) > _watchdogTimeout)
                {
                    _monitoringEvents.Warning("Did not receive any game state update since {Timestamp}, re-initiating the connection", _dataWatchdogTimestamp);
                    _dataWatchdogTimestamp = DateTime.UtcNow;

                    var credentials = _credentialCache.LoadAsync().GetAwaiter().GetResult();
                    if (credentials == null || string.IsNullOrEmpty(credentials.AccessToken))
                    {
                        _monitoringEvents.Error("No Zwift credentials available, cannot initiate Zwift connection");
                        return;
                    }

                    await _connectUseCase
                        .ExecuteAsync(new ConnectCommand
                        {
                            ConnectionEncryptionSecret = _userPreferences.ConnectionSecret,
                            AccessToken = credentials.AccessToken!
                        });
                }
            }
        }
    }
}

// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;
using RoadCaptain.Commands;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;
using Serilog;
using Serilog.Core;

namespace RoadCaptain.Runner
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class App : Application
    {
        private readonly IContainer _container;
        private readonly Logger _logger;
        private readonly CancellationTokenSource _tokenSource = new();
        private IGameStateReceiver _gameStateReceiver;
        private Task _gameStateReceiverTask;
        private Task _listenerTask;
        private Task _initiatorTask;
        private CancellationTokenSource _connectionToken = new();
        private CancellationTokenSource _messageHandlingToken;
        private Task _messageHandlingTask;
        private CancellationTokenSource _navigationToken;
        private Task _navigationTask;
        private GameState _previousGameState;

        public App()
        {
            _logger = LoggerBootstrapper.CreateLogger();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("autofac.runner.json")
                .AddJsonFile("autofac.runner.development.json", true)
                .Build();

            var builder = new ContainerBuilder();

            builder.Register<ILogger>(_ => _logger).SingleInstance();
            builder.Register<IConfiguration>(_ => configuration).SingleInstance();

            builder.RegisterType<Configuration>().AsSelf().SingleInstance();

            builder.Register(_ => AppSettings.Default).SingleInstance();

            // Wire up registrations through the autofac.json file
            builder.RegisterModule(new ConfigurationModule(configuration));

            _container = builder.Build();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _gameStateReceiver = _container.Resolve<IGameStateReceiver>();
            _gameStateReceiver.Register(null, null, GameStateReceived);
            _gameStateReceiverTask = Task.Factory.StartNew(
                () => _gameStateReceiver.Start(_tokenSource.Token),
                TaskCreationOptions.LongRunning);

            var mainWindow = _container.Resolve<MainWindow>();

            mainWindow.Show();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            CancelAndCleanUp(_tokenSource, () => _gameStateReceiverTask);

            // Flush the logger
            _logger?.Dispose();
        }

        private void GameStateReceived(GameState gameState)
        {
            _logger.Debug("Received game state of type {GameStateType}", gameState.GetType().Name);

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

                // TODO: load the route

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
                _container.Resolve<IWindowService>().ShowErrorDialog(errorState.Exception.Message);
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

            var listenerUseCase = _container.Resolve<DecodeIncomingMessagesUseCase>();

            _listenerTask = Task.Factory.StartNew(() => listenerUseCase.ExecuteAsync(_connectionToken.Token), TaskCreationOptions.LongRunning);
        }

        private void StartZwiftConnectionInitiator()
        {
            if (_initiatorTask.IsRunning())
            {
                return;
            }

            _logger.Information("Starting connection initiator");

            var connectUseCase = _container.Resolve<ConnectToZwiftUseCase>();

            _initiatorTask = Task.Factory.StartNew(() =>
                    connectUseCase
                        .ExecuteAsync(
                            new ConnectCommand { AccessToken = _container.Resolve<Configuration>().AccessToken },
                            _connectionToken.Token),
                TaskCreationOptions.LongRunning);
        }

        private void StartMessageHandler()
        {
            _logger.Information("Starting message handler");

            var handleMessageUseCase = _container.Resolve<HandleZwiftMessagesUseCase>();
            _messageHandlingToken = new CancellationTokenSource();
            _messageHandlingTask = Task.Factory.StartNew(
                () => handleMessageUseCase.Execute(_messageHandlingToken.Token),
                TaskCreationOptions.LongRunning);
        }

        private void StartNavigation()
        {
            _logger.Information("Starting navigation");

            var navigationUseCase = _container.Resolve<NavigationUseCase>();
            _navigationToken = new CancellationTokenSource();
            _navigationTask = Task.Factory.StartNew(
                () => navigationUseCase.Execute(_navigationToken.Token),
                TaskCreationOptions.LongRunning);
        }
    }
}
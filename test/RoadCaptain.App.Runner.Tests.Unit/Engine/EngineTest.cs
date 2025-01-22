// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Avalonia.Threading;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using RoadCaptain.App.Runner.Tests.Unit.ViewModels;
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.Models;
using RoadCaptain.GameStates;
using RoadCaptain.Ports;
using Serilog;
using Serilog.Sinks.InMemory;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class EngineTest : IDisposable
    {
        private readonly TaskWithCancellation _receiverTask;
        private readonly IGameStateReceiver _gameStateReceiver;
        private readonly InMemoryZwiftGameConnection _zwiftGameConnection;
        protected IUserPreferences UserPreferences;
        private readonly IGameStateDispatcher _gameStateDispatcher;

        public EngineTest()
        {
            InMemorySink = new InMemorySink();

            var logger = new LoggerConfiguration()
                .WriteTo.Sink(InMemorySink)
                .Enrich.FromLogContext()
                .CreateLogger();
            
            var configuration = new ConfigurationBuilder()
                .Build();

            var containerBuilder = InversionOfControl.ConfigureContainer(configuration, logger, Dispatcher.UIThread);

            containerBuilder.RegisterModule<TestModule>();

            // Register the testable engine so we can properly resolve it
            containerBuilder.RegisterType<TestableEngine>().AsSelf();
            containerBuilder.RegisterType<InMemoryZwiftCredentialCache>().As<IZwiftCredentialCache>().SingleInstance();
            
            var container = containerBuilder.Build();
            
            container.Resolve<Configuration>().Route = "someroute.json";
            
            UserPreferences = container.Resolve<IUserPreferences>();

            _gameStateDispatcher = container.Resolve<IGameStateDispatcher>();

            _gameStateReceiver = container.Resolve<IGameStateReceiver>();
            _gameStateReceiver.ReceiveRoute(route =>
            {
                LoadedRoute = route;
            });
            _gameStateReceiver.ReceiveGameState(state => States.Add(state));

            _receiverTask = TaskWithCancellation.Start(token =>
            {
                _gameStateReceiver.Start(token);
                return Task.CompletedTask;
            });

            _zwiftGameConnection = (container.Resolve<IZwiftGameConnection>() as InMemoryZwiftGameConnection)!;
            
            // Ensure some credentials are available
            container
                .Resolve<IZwiftCredentialCache>()
                .StoreAsync(new TokenResponse { AccessToken = "NOTIMPORTANT", UserProfile = new UserProfile { FirstName = "Test", LastName = "User"}})
                .GetAwaiter()
                .GetResult();

            Engine = container.Resolve<TestableEngine>();
            WindowService = container.Resolve<StubWindowService>();
        }

        protected StubWindowService WindowService { get; }

        protected PlannedRoute? LoadedRoute { get; private set; }

        protected List<GameState> States { get; } = new();

        protected TestableEngine Engine { get; }

        protected InMemorySink InMemorySink { get; }
        protected List<string> SentCommands => _zwiftGameConnection.SentCommands;

        public void Dispose()
        {
            _receiverTask.Cancel();

            try
            {
                Engine.Stop();
            }
            catch
            {
                // ignored
            }
        }

        protected void ReceiveGameState(GameState state)
        {
            Engine.PushState(state);

            // Ensure that the dispatcher processed
            // all messages.
            _gameStateReceiver.Drain();
        }

        protected void LoadRoute(PlannedRoute plannedRoute)
        {
            _gameStateDispatcher.RouteSelected(plannedRoute);
        }

        protected TaskWithCancellation? TheTaskWithName(string fieldName)
        {
            var value = GetFieldValueByName(fieldName);

            if (value == null)
            {
                return null;
            }

            value.Should().BeOfType<TaskWithCancellation>();

            return value as TaskWithCancellation;
        }

        protected object? GetFieldValueByName(string fieldName)
        {
            var fieldInfo = typeof(Runner.Engine)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);

            fieldInfo.Should().NotBeNull($"the field '{fieldName}' should exist");

            var fieldValueByName = fieldInfo!.GetValue(Engine);

            return fieldValueByName;
        }

        protected void SetFieldValueByName(string fieldName, object value)
        {
            var fieldInfo = typeof(Runner.Engine)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.SetField | BindingFlags.NonPublic);
            
            fieldInfo.Should().NotBeNull($"the field '{fieldName}' should exist");
            
            fieldInfo!.SetValue(Engine, value);
        }

        protected TaskWithCancellation GivenTaskIsRunning(string fieldName)
        {
            var taskWithCancellation = TaskWithCancellation.Start(token => Task.FromResult(token.WaitHandle.WaitOne()));

            SetFieldValueByName(fieldName, taskWithCancellation);

            return taskWithCancellation;
        }
    }
}

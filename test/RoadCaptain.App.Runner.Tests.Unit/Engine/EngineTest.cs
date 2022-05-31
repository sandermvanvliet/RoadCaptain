using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using RoadCaptain.App.Runner.Tests.Unit.ViewModels;
using RoadCaptain.App.Shared;
using RoadCaptain.App.Shared.UserPreferences;
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
        private readonly ClassicDesktopStyleApplicationLifetime _lifetime;
        private readonly InMemoryZwiftGameConnection _zwiftGameConnection;
        protected IUserPreferences UserPreferences;

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
            
            var container = containerBuilder.Build();
            
            container.Resolve<Configuration>().Route = "someroute.json";
            
            UserPreferences = container.Resolve<IUserPreferences>();

            _gameStateReceiver = container.Resolve<IGameStateReceiver>();
            _gameStateReceiver
                .Register(
                    route =>
                    {
                        LoadedRoute = route;
                    },
                    null,
                    state => States.Add(state));

            _receiverTask = TaskWithCancellation.Start(token => _gameStateReceiver.Start(token));

            _zwiftGameConnection = container.Resolve<IZwiftGameConnection>() as InMemoryZwiftGameConnection;

            Engine = container.Resolve<TestableEngine>();
            WindowService = container.Resolve<StubWindowService>();

            _lifetime = new ClassicDesktopStyleApplicationLifetime();
            WindowService.SetLifetime(_lifetime);

            AvaloniaTestSupport.MockServices();
        }

        protected StubWindowService WindowService { get; }

        protected PlannedRoute LoadedRoute { get; private set; }

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
            
            _lifetime?.Dispose();
        }

        protected void ReceiveGameState(GameState state)
        {
            Engine.PushState(state);

            // Ensure that the dispatcher processed
            // all messages.
            _gameStateReceiver.Drain();
        }

        protected TaskWithCancellation TheTaskWithName(string fieldName)
        {
            return GetFieldValueByName(fieldName) as TaskWithCancellation;
        }

        protected object GetFieldValueByName(string fieldName)
        {
            var fieldInfo = typeof(Runner.Engine)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);

            // ReSharper disable once PossibleNullReferenceException
            return fieldInfo.GetValue(Engine);
        }

        protected void SetFieldValueByName(string fieldName, object value)
        {
            var fieldInfo = typeof(Runner.Engine)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.SetField | BindingFlags.NonPublic);

            // ReSharper disable once PossibleNullReferenceException
            fieldInfo.SetValue(Engine, value);
        }

        protected TaskWithCancellation GivenTaskIsRunning(string fieldName)
        {
            var taskWithCancellation = TaskWithCancellation.Start(token => token.WaitHandle.WaitOne());

            SetFieldValueByName(fieldName, taskWithCancellation);

            return taskWithCancellation;
        }
    }
}
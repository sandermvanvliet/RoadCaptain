using System;
using System.Collections.Generic;
using System.Reflection;
using RoadCaptain.Adapters;
using RoadCaptain.GameStates;
using RoadCaptain.Runner.Tests.Unit.ViewModels;
using RoadCaptain.UseCases;
using Serilog;
using Serilog.Sinks.InMemory;

namespace RoadCaptain.Runner.Tests.Unit.Engine
{
    public class EngineTest : IDisposable
    {
        private readonly TaskWithCancellation _receiverTask;

        public EngineTest()
        {
            InMemorySink = new InMemorySink();

            var logger = new LoggerConfiguration()
                .WriteTo.Sink(InMemorySink)
                .Enrich.FromLogContext()
                .CreateLogger();

            var monitoringEvents = new MonitoringEventsWithSerilog(logger);

            var configuration = new Configuration(null)
            {
                Route = "someroute.json"
            };

            var gameStateDispatcher = new InMemoryGameStateDispatcher(monitoringEvents);
            gameStateDispatcher
                .Register(
                    route => LoadedRoute = route,
                    null,
                    state => States.Add(state));

            var messageEmitter = new MessageEmitterToQueue(monitoringEvents, new MessageEmitterConfiguration(null));
            WindowService = new StubWindowService();

            Engine = new TestableEngine(
                monitoringEvents,
                new LoadRouteUseCase(gameStateDispatcher, new StubRouteStore()),
                configuration,
                WindowService,
                new DecodeIncomingMessagesUseCase(new StubMessageReceiver(), messageEmitter, monitoringEvents),
                null,
                new HandleZwiftMessagesUseCase(messageEmitter, monitoringEvents, new SegmentStore(),
                    gameStateDispatcher, new NopZwiftGameConnection(), gameStateDispatcher),
                null,
                gameStateDispatcher
            );

            _receiverTask = TaskWithCancellation.Start(token => gameStateDispatcher.Start(token));
        }

        protected StubWindowService WindowService { get; }

        protected PlannedRoute LoadedRoute { get; private set; }

        protected List<GameState> States { get; } = new();

        protected TestableEngine Engine { get; }

        protected InMemorySink InMemorySink { get; }

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
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RoadCaptain.Adapters;
using RoadCaptain.GameStates;
using Serilog;
using Serilog.Sinks.InMemory;

namespace RoadCaptain.Runner.Tests.Unit.Engine
{
    public class EngineTest
    {
        private readonly InMemoryGameStateDispatcher _gameStateDispatcher;

        public EngineTest()
        {
            InMemorySink = new InMemorySink();

            var logger = new LoggerConfiguration()
                .WriteTo.Sink(InMemorySink)
                .Enrich.FromLogContext()
                .CreateLogger();

            var monitoringEvents = new MonitoringEventsWithSerilog(logger);

            _gameStateDispatcher = new InMemoryGameStateDispatcher(monitoringEvents);
            
            Engine = new Runner.Engine(
                monitoringEvents,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                _gameStateDispatcher
            );
        }

        protected Runner.Engine Engine { get; }

        protected InMemorySink InMemorySink { get; }

        protected void ReceiveGameState(GameState state)
        {
            _gameStateDispatcher.Dispatch(state);

            var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
            
            _gameStateDispatcher.Register(null, null, _ => tokenSource.Cancel());

            _gameStateDispatcher.Start(tokenSource.Token);
        }

        protected TaskWithCancellation GetTaskByFieldName(string fieldName)
        {
            return GetFieldValueByName(fieldName) as TaskWithCancellation;
        }

        protected object GetFieldValueByName(string fieldName)
        {
            var fieldInfo = Engine
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);

            return fieldInfo.GetValue(Engine);
        }

        protected void SetFieldValueByName(string fieldName, object value)
        {
            var fieldInfo = Engine
            .GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.SetField | BindingFlags.NonPublic);
            
            fieldInfo.SetValue(Engine, value);
        }
    }
}
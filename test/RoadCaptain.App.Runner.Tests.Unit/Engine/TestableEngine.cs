using RoadCaptain.GameStates;
using RoadCaptain.Ports;
using RoadCaptain.UseCases;

namespace RoadCaptain.App.Runner.Tests.Unit.Engine
{
    public class TestableEngine : Runner.Engine
    {
        public TestableEngine(MonitoringEvents monitoringEvents, LoadRouteUseCase loadRouteUseCase,
            Configuration configuration, IWindowService windowService, DecodeIncomingMessagesUseCase listenerUseCase,
            ConnectToZwiftUseCase connectUseCase, HandleZwiftMessagesUseCase handleMessageUseCase,
            NavigationUseCase navigationUseCase, IGameStateReceiver gameStateReceiver, ISegmentStore segmentStore,
            IZwiftGameConnection zwiftGameConnection, IUserPreferences userPreferences,
            IZwiftCredentialCache credentialCache) :
            base(monitoringEvents,
                loadRouteUseCase, configuration, windowService, listenerUseCase, connectUseCase, handleMessageUseCase,
                navigationUseCase, gameStateReceiver, segmentStore, zwiftGameConnection, userPreferences, credentialCache)
        {
        }

        public void PushState(GameState state)
        {
            GameStateReceived(state);
        }
    }
}
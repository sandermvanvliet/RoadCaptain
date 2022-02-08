using System.Collections.Generic;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class HandleActivityDetailsUseCase
    {
        private readonly IGameStateDispatcher _dispatcher;

        public HandleActivityDetailsUseCase(IGameStateDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Execute(ZwiftActivityDetailsMessage activityDetails)
        {
            if (_dispatcher.InGame && activityDetails.ActivityId == 0)
            {
                _dispatcher.LeaveGame();
                _dispatcher.SegmentChanged(null);
                _dispatcher.TurnsAvailable(new List<Turn>());
                _dispatcher.DirectionChanged(SegmentDirection.Unknown);
            }
            else if (!_dispatcher.InGame && activityDetails.ActivityId != 0)
            {
                _dispatcher.EnterGame(activityDetails.ActivityId);
            }
        }
    }
}
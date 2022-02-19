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
            }
            else if (!_dispatcher.InGame && activityDetails.ActivityId != 0)
            {
                _dispatcher.EnterGame(activityDetails.ActivityId);
            }
        }
    }
}
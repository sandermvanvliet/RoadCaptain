namespace RoadCaptain.GameStates
{
    public class NotInGameState : GameState
    {
        public GameState EnterGame(int activityId)
        {
            return new InGameState(activityId);
        }
    }
}
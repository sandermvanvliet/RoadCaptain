namespace RoadCaptain.GameState
{
    public class NotInGameState : GameState
    {
        public GameState EnterGame(int activityId)
        {
            return new InGameState(activityId);
        }
    }
}
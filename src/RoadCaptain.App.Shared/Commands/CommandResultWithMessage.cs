namespace RoadCaptain.App.Shared.Commands
{
    public class CommandResultWithMessage : CommandResult
    {
        public CommandResultWithMessage(Result result, string message)
        {
            Result = result;
            Message = message;
        }
        public string Message { get; set; }
    }
}
namespace RoadCaptain.App.Shared.Commands
{
    public class CommandResult
    {
        public Result Result { get; set; }

        public static CommandResult Success()
        {
            return new() { Result = Result.Success };
        }

        public static CommandResultWithMessage SuccessWithMessage(string message)
        {
            return new CommandResultWithMessage(Result.SuccessWithWarnings, message);
        }

        public static CommandResultWithMessage Failure(string message)
        {
            return new CommandResultWithMessage(Result.Failure, message);
        }

        public static CommandResult Aborted()
        {
            return new CommandResult { Result = Result.NotExecuted };
        }
    }
}
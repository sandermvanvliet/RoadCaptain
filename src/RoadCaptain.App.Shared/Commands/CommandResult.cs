// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
            return new CommandResultWithMessage(Result.SuccessWithMessage, message);
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

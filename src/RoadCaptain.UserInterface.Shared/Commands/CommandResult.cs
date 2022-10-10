// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.UserInterface.Shared.Commands
{
    public class CommandResult
    {
        public Result Result { get; set; }
        public string Message { get; set; }

        public static CommandResult Success(string message = null)
        {
            return new() { Result = Result.Success, Message = message };
        }

        public static CommandResult SuccessWithWarning(string message)
        {
            return new() { Result = Result.SuccessWithWarnings, Message = message };
        }

        public static CommandResult Failure(string message)
        {
            return new CommandResult { Result = Result.Failure, Message = message };
        }

        public static CommandResult Aborted()
        {
            return new CommandResult { Result = Result.NotExecuted };
        }
    }
}

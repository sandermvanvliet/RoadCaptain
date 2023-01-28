// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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

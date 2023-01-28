// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;

namespace RoadCaptain.App.Shared
{
    public static class TaskExtensions
    {
        public static void SafeWaitForCancellation(this Task? task)
        {
            if (task == null)
            {
                return;
            }

            try
            {
                task.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                // Nop
            }
        }

        public static bool IsRunning(this TaskWithCancellation? task)
        {
            return task?.Task is
            {
                Status:
                TaskStatus.Created or
                TaskStatus.WaitingForActivation or
                TaskStatus.WaitingToRun or
                TaskStatus.Running
            };
        }
    }
}


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

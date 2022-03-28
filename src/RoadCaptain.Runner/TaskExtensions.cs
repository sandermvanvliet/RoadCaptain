using System;
using System.Threading.Tasks;

namespace RoadCaptain.Runner
{
    public static class TaskExtensions
    {
        public static void SafeWaitForCancellation(this Task task)
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

        public static bool IsRunning(this Task task)
        {
            return task is
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

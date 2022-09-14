using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoadCaptain.App.Shared
{
    public class TaskWithCancellation
    {
        private readonly CancellationTokenSource _tokenSource;

        private TaskWithCancellation(Func<CancellationToken, Task> action)
            : this(new CancellationTokenSource(), action)
        {
        }

        private TaskWithCancellation(CancellationTokenSource tokenSource, Func<CancellationToken, Task> action)
        {
            _tokenSource = tokenSource;

            Task = Task.Factory.StartNew(
                () => action(_tokenSource.Token),
                TaskCreationOptions.LongRunning);
        }

        public Task Task { get; }

        public void Cancel()
        {
            if (!_tokenSource.IsCancellationRequested)
            {
                _tokenSource.Cancel();
            }

            Task.SafeWaitForCancellation();
        }

        public TaskWithCancellation StartLinkedTask(Func<CancellationToken, Task> action)
        {
            return new TaskWithCancellation(_tokenSource, action);
        }

        public static TaskWithCancellation Start(Func<CancellationToken, Task> action)
        {
            return new TaskWithCancellation(action);
        }
    }
}
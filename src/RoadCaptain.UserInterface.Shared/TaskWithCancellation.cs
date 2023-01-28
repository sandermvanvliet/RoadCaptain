// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoadCaptain.UserInterface.Shared
{
    public class TaskWithCancellation
    {
        private readonly CancellationTokenSource _tokenSource;

        private TaskWithCancellation(Action<CancellationToken> action)
            : this(new CancellationTokenSource(), action)
        {
        }

        private TaskWithCancellation(CancellationTokenSource tokenSource, Action<CancellationToken> action)
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

        public TaskWithCancellation StartLinkedTask(Action<CancellationToken> action)
        {
            return new TaskWithCancellation(_tokenSource, action);
        }

        public static TaskWithCancellation Start(Action<CancellationToken> action)
        {
            return new TaskWithCancellation(action);
        }
    }
}

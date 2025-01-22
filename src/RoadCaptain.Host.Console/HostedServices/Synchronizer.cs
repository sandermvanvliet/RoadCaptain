// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoadCaptain.Host.Console.HostedServices
{
    internal class Synchronizer : ISynchronizer
    {
        private static readonly object SyncRoot = new();
        private readonly List<Action> _stopCallbacks = new();
        private readonly List<Func<Task>> _synchronizedStarts = new();
        private bool _synchronized;

        public bool Synchronized
        {
            get => _synchronized;
            set
            {
                lock (SyncRoot)
                {
                    _synchronized = value;
                }
            }
        }

        public void RegisterStop(Action action)
        {
            _stopCallbacks.Add(action);
        }

        public void RequestApplicationStop()
        {
            foreach (var callback in _stopCallbacks)
            {
                try
                {
                    callback();
                }
                catch
                {
                    // Ignore this
                }
            }
        }

        public void RegisterStart(Func<Task> func)
        {
            // When a service attempts to register after the synchronization event
            // was triggered, the callback can be invoked immediately.
            if (Synchronized)
            {
                func();
            }
            else
            {
                _synchronizedStarts.Add(func);
            }
        }

        public void TriggerSynchronizationEvent()
        {
            if (Synchronized)
            {
                // We will only trigger the event once
                return;
            }

            Synchronized = true;

            foreach (var x in _synchronizedStarts)
            {
                x().GetAwaiter().GetResult();
            }
        }
    }
}

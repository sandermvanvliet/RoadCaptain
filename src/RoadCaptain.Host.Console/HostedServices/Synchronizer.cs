using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RoadCaptain.Host.Console.HostedServices
{
    internal class Synchronizer : ISynchronizer
    {
        private readonly List<Tuple<Func<CancellationToken, Task>, CancellationToken>> _synchronizedStarts = new();
        private bool _synchronized;
        private static readonly object SyncRoot = new();
        private readonly List<Action> _stopCallbacks = new();

        public void RegisterStop(Action action)
        {
            _stopCallbacks.Add(action);
        }

        public void Stop()
        {
            foreach (var callback in _stopCallbacks)
            {
                try
                {
                    callback();
                }
                catch 
                {
                }
            }
        }

        public void RegisterStart(Func<CancellationToken, Task> func, CancellationToken cancellationToken)
        {
            if (Synchronized)
            {
                Task.Factory.StartNew(() => func(cancellationToken));
            }
            else
            {
                _synchronizedStarts.Add(new(func, cancellationToken));
            }
        }

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

        public void Start()
        {
            Synchronized = true;

            foreach (var x in _synchronizedStarts)
            {
                Task.Factory.StartNew(() => x.Item1(x.Item2));
            }
        }
    }
}
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
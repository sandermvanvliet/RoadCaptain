using System;
using System.Collections.Generic;

namespace RoadCaptain.App.RouteBuilder.Services
{
    public class StatusBarService : IStatusBarService
    {
        private readonly List<WeakReference<Action<string>>> _infoHandlers = new();
        private readonly List<WeakReference<Action<string>>> _warningHandlers = new();
        private readonly List<WeakReference<Action<string>>> _errorHandlers = new();

        public void Info(string message)
        {
            foreach (var handler in _infoHandlers)
            {
                if (handler.TryGetTarget(out var handlerTarget))
                {
                    handlerTarget.Invoke(message);
                }
            }
        }
        
        public void Warning(string message)
        {
            foreach (var handler in _warningHandlers)
            {
                if (handler.TryGetTarget(out var handlerTarget))
                {
                    handlerTarget.Invoke(message);
                }
            }
        }
        
        public void Error(string message)
        {
            foreach (var handler in _errorHandlers)
            {
                if (handler.TryGetTarget(out var handlerTarget))
                {
                    handlerTarget.Invoke(message);
                }
            }
        }

        public void Subscribe(Action<string> info, Action<string> warning, Action<string> error)
        {
            _infoHandlers.Add(new WeakReference<Action<string>>(info));
            _warningHandlers.Add(new WeakReference<Action<string>>(info));
            _errorHandlers.Add(new WeakReference<Action<string>>(info));
        }
    }

    public interface IStatusBarService
    {
        void Subscribe(Action<string> info, Action<string> warning, Action<string> error);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
    }
}
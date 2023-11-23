// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;

namespace RoadCaptain.App.RouteBuilder.Services
{
    public class StatusBarService : IStatusBarService
    {
        private readonly List<Action<string>> _infoHandlers = new();
        private readonly List<Action<string>> _warningHandlers = new();
        private readonly List<Action<string>> _errorHandlers = new();

        public void Info(string message)
        {
            foreach (var handler in _infoHandlers)
            {
                try
                {
                    handler.Invoke(message);
                }
                catch
                {
                    // nop
                }
            }
        }
        
        public void Warning(string message)
        {
            foreach (var handler in _warningHandlers)
            {
                try
                {
                    handler.Invoke(message);
                }
                catch
                {
                    // nop
                }
            }
        }
        
        public void Error(string message)
        {
            foreach (var handler in _errorHandlers)
            {
                try
                {
                    handler.Invoke(message);
                }
                catch
                {
                    // nop
                }
            }
        }

        public void Subscribe(Action<string> info, Action<string> warning, Action<string> error)
        {
            _infoHandlers.Add(info);
            _warningHandlers.Add(warning);
            _errorHandlers.Add(error);
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

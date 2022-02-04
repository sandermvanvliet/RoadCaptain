﻿using System.Collections.Generic;
using RoadCaptain.Ports;

namespace RoadCaptain.UseCases
{
    public class HandleAvailableTurnsUseCase
    {
        private readonly List<TurnDirection> _availableTurnCommands = new();
        private readonly MonitoringEvents _monitoringEvents;
        private readonly IGameStateDispatcher _dispatcher;

        public HandleAvailableTurnsUseCase(MonitoringEvents monitoringEvents, IGameStateDispatcher dispatcher)
        {
            _monitoringEvents = monitoringEvents;
            _dispatcher = dispatcher;
        }

        public void Execute(ZwiftCommandAvailableMessage commandAvailable)
        {
            var startCount = _availableTurnCommands.Count;

            switch (commandAvailable.Type.Trim().ToLower())
            {
                case "turnleft":
                    if (!_availableTurnCommands.Contains(TurnDirection.Left))
                    {
                        _availableTurnCommands.Add(TurnDirection.Left);
                    }

                    break;
                case "turnright":
                    if (!_availableTurnCommands.Contains(TurnDirection.Right))
                    {
                        _availableTurnCommands.Add(TurnDirection.Right);
                    }

                    break;
                case "gostraight":
                    if (!_availableTurnCommands.Contains(TurnDirection.StraightOn))
                    {
                        _availableTurnCommands.Add(TurnDirection.StraightOn);
                    }

                    break;
            }

            // Only call the dispatcher when the number of turns changed and
            // there are at least two turns available.
            if (startCount != _availableTurnCommands.Count && _availableTurnCommands.Count >= 2)
            {
                _dispatcher.TurnCommandsAvailable(_availableTurnCommands);
            }

            // TODO: reset on segment changes
            // Or perhaps don't because on a new segment the dispatcher already
            // clears these and we'll receive new ones when the rider gets to 
            // upcoming turns on the new segment...
        }
    }
}
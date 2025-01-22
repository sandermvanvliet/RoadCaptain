// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using RoadCaptain.Ports;

namespace RoadCaptain.Tests.Unit
{
    public class NopGameStateDispatcher : IGameStateDispatcher
    {
        public void RouteSelected(PlannedRoute route)
        {
            throw new NotImplementedException();
        }

        public void UpdateLastSequenceNumber(ulong sequenceNumber)
        {
            throw new NotImplementedException();
        }

        public void LoggedIn()
        {
            throw new NotImplementedException();
        }

        public void WaitingForConnection()
        {
            throw new NotImplementedException();
        }

        public void Connected()
        {
            throw new NotImplementedException();
        }

        public void EnterGame(uint riderId, ulong activityId)
        {
            throw new NotImplementedException();
        }

        public void LeaveGame()
        {
            throw new NotImplementedException();
        }

        public void UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            throw new NotImplementedException();
        }

        public void TurnCommandAvailable(string type)
        {
            throw new NotImplementedException();
        }

        public void Error(Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Error(string message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void InvalidCredentials(Exception exception)
        {
            throw new NotImplementedException();
        }

        public void StartRoute()
        {
            throw new NotImplementedException();
        }

        public void IncorrectConnectionSecret()
        {
            throw new NotImplementedException();
        }
    }
}

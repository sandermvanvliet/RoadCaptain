using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RoadCaptain.GameStates
{
    public class ErrorState : GameState
    {
        public ErrorState(Exception exception)
        {
            Exception = exception;
        }

        public ErrorState(Exception exception, uint riderId)
        {
            Exception = exception;
            RiderId = riderId;
        }

        public override uint RiderId { get; }

        [JsonProperty]
        public Exception Exception { get; private set; }

        public override GameState EnterGame(uint riderId, ulong activityId)
        {
            throw new NotImplementedException();
        }

        public override GameState LeaveGame()
        {
            throw new NotImplementedException();
        }

        public override GameState UpdatePosition(TrackPoint position, List<Segment> segments, PlannedRoute plannedRoute)
        {
            throw new NotImplementedException();
        }

        public override GameState TurnCommandAvailable(string type)
        {
            throw new NotImplementedException();
        }
    }
}
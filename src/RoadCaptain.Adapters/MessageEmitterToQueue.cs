using System;
using System.Diagnostics;
using System.Linq;
using Google.Protobuf;
using RoadCaptain.Adapters.Protobuf;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class MessageEmitterToQueue : IMessageEmitter
    {
        private readonly MonitoringEvents _monitoringEvents;

        /// <summary>
        /// Raised when the Zwift desktop app indicates that a command has become available to the companion app
        /// </summary>
        public event EventHandler<CommandAvailableEventArgs> CommandAvailable;

        /// <summary>
        /// Raised when the position of the rider is sent to the companion app
        /// </summary>
        public event EventHandler<RiderPositionEventArgs> RiderPosition;

        /// <summary>
        /// Raised when a power up is rewarded in the game
        /// </summary>
        public event EventHandler<PowerUpEventArgs> PowerUp;

        /// <summary>
        /// Raised when activity details are received
        /// </summary>
        public event EventHandler<ActivityDetailsEventArgs> ActivityDetails;
        
        /// <summary>
        /// Raised when Zwift pings the companion app
        /// </summary>
        public event EventHandler<PingEventArgs> Ping;

        public MessageEmitterToQueue(MonitoringEvents monitoringEvents)
        {
            _monitoringEvents = monitoringEvents;
        }

        public void EmitMessageFromBytes(byte[] payload, long sequenceNumber)
        {
            if (payload == null || payload.Length == 0)
            {
                _monitoringEvents.Debug("Ignoring empty message payload for sequence no {SequenceNumber}", sequenceNumber);
                return;
            }

            ZwiftAppToCompanion packetData;

            try
            {
                packetData = ZwiftAppToCompanion.Parser.ParseFrom(payload);
            }
            catch (InvalidProtocolBufferException ex)
            {
                _monitoringEvents.Warning(ex, "Invalid protobuf message: {Message}", ex.Message);
                return;
            }

            try
            {
                DecodeIncomingCore(packetData);
            }
            catch (Exception e)
            {
                _monitoringEvents.Error(e, "Failed to decode incoming message");
                Debugger.Break();
            }
        }

        public void SubscribeOnPing(Action<int> callback)
        {
            Ping += (_, e) => callback((int)e.RiderId);
        }

        private void DecodeIncomingCore(ZwiftAppToCompanion packetData)
        {
            if (packetData.Items == null || !packetData.Items.Any())
            {
                // We need to capture the first one to send an initialization message
                // back to Zwift so that it will start feeding us the actual data.
                OnZwiftPing(packetData);

                _monitoringEvents.Debug("Ignoring message with empty item collection");
                return;
            }

            foreach (var item in packetData.Items)
            {
                var byteArray = item.ToByteArray();

                switch (item.Type)
                {
                    case 1:
                    case 3:
                    case 6:
                        // Empty, ignore this
                        break;
                    case 2:
                        var powerUp = ZwiftAppToCompanionPowerUpMessage.Parser.ParseFrom(byteArray);
                        
                        OnPowerUp(powerUp.PowerUp);

                        break;
                    case 4:
                        var buttonMessage = ZwiftAppToCompanionButtonMessage.Parser.ParseFrom(byteArray);

                        OnCommandAvailable(buttonMessage.TypeId, buttonMessage.Title);

                        break;
                    case 9:
                        _monitoringEvents.Information("Received a type 9 message that we don't understand yet");
                        break;
                    case 13:
                        var activityDetails = ZwiftAppToCompanionActivityDetailsMessage.Parser.ParseFrom(byteArray);

                        DecodeIncomingActivityDetailsMessage(activityDetails);

                        break;
                    default:
                        _monitoringEvents.Warning("Received type {Type} message that we don't understand yet", item.Type);

                        break;
                }
            }
        }

        private void DecodeIncomingActivityDetailsMessage(ZwiftAppToCompanionActivityDetailsMessage activityDetails)
        {
            switch (activityDetails.Details.Type)
            {
                case 3:
                    OnActivityDetails(activityDetails.Details.Data.ActivityId);
                    break;
                case 5:
                    {
                        // This item type either has our position or that of a bunch of others
                        // so let's first see if we can deal with our position first.
                        if (activityDetails.Details.RiderData.Sub.Count == 1 &&
                            activityDetails.Details.RiderData.Sub[0].Index == 10)
                        {
                            var rider = activityDetails.Details.RiderData.Sub[0].Riders[0];

                            OnRiderPosition(
                                rider.Position.Latitude,
                                rider.Position.Longitude,
                                rider.Position.Altitude);

                            break;
                        }

                        foreach (var s in activityDetails.Details.RiderData.Sub)
                        {
                            if (s?.Riders != null && s.Riders.Any())
                            {
                                foreach (var rider in s.Riders)
                                {
                                    var subject = $"{rider.Description} ({rider.RiderId})";

                                    _monitoringEvents.Debug($"Received rider information: {subject}");
                                }
                            }
                        }

                        break;
                    }
                case 6:
                    // Ignore
                    break;
                case 7:
                    // Ignore, comes by lots of times
                    break;
                case 10:
                    // Ignore, this has very limited data that I have no idea about what it means
                    break;
                case 18:
                    // Ignore, this has very limited data that I have no idea about what it means
                    break;
                case 17:
                case 19:
                    // Rider nearby?
                    {
                        var rider = activityDetails
                            .Details
                            ?.OtherRider;

                        if (rider != null)
                        {
                            var subject = $"{rider.FirstName?.Trim()} {rider.LastName?.Trim()} ({rider.RiderId})";

                            _monitoringEvents.Debug("Received rider nearby position for {Subject}", subject);
                        }

                        break;
                    }
                case 20:
                    // Ignore, contains very little data and is similar to type 21
                    break;
                case 21:
                    // Ignore, contains very little data
                    break;
                default:
                    _monitoringEvents.Debug($"Received a activity details subtype with {activityDetails.Details.Type} that we don't understand yet");

                    break;
            }
        }

        private void OnZwiftPing(ZwiftAppToCompanion zwiftAppToCompanion)
        {
            try
            {
                Ping?.Invoke(this, new PingEventArgs
                {
                    RiderId = zwiftAppToCompanion.RiderId
                });
            }
            catch
            {
                // Ignore exceptions from event handlers.
            }
        }

        private void OnActivityDetails(ulong activityId)
        {
            try
            {
                ActivityDetails?.Invoke(this, new ActivityDetailsEventArgs
                {
                    ActivityId = activityId
                });
            }
            catch
            {
                // Ignore exceptions from event handlers.
            }
        }

        private void OnPowerUp(string type)
        {
            try
            {
                PowerUp?.Invoke(this, new PowerUpEventArgs { Type = type });
            }
            catch
            {
                // Ignore exceptions from event handlers.
            }
        }

        private void OnCommandAvailable(uint numericalCommandType, string description)
        {
            var commandType = CommandType.Unknown;

            try
            {
                commandType = (CommandType)numericalCommandType;
            }
            catch
            {
                // Nop
            }

            if (commandType == CommandType.Unknown)
            {
                _monitoringEvents.Warning($"Did not recognise command {numericalCommandType} ({description})");
            }

            try
            {
                CommandAvailable?.Invoke(this, new CommandAvailableEventArgs { CommandType = commandType });
            }
            catch
            {
                // Ignore exceptions from event handlers.
            }
        }

        private void OnRiderPosition(float latitude, float longitude, float altitude)
        {
            try
            {
                RiderPosition?.Invoke(this,
                    new RiderPositionEventArgs { Latitude = latitude, Longitude = longitude, Altitude = altitude });
            }
            catch
            {
                // Ignore exceptions from event handlers.
            }
        }
    }
}
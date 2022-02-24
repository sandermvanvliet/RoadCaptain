using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Google.Protobuf;
using RoadCaptain.Adapters.Protobuf;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class MessageEmitterToQueue : IMessageEmitter
    {
        private readonly MonitoringEvents _monitoringEvents;
        private readonly MessageEmitterConfiguration _configuration;
        private readonly ConcurrentQueue<ZwiftMessage> _queue = new();
        private readonly AutoResetEvent _autoResetEvent = new(false);
        private readonly TimeSpan _queueWaitTimeout = TimeSpan.FromMilliseconds(2000);
        private readonly AutoResetEvent _throttleResetEvent = new(false);

        public MessageEmitterToQueue(
            MonitoringEvents monitoringEvents,
            MessageEmitterConfiguration configuration)
        {
            _monitoringEvents = monitoringEvents;
            _configuration = configuration;
        }

        public void EmitMessageFromBytes(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
            {
                _monitoringEvents.Debug("Ignoring empty message payload");
                return;
            }

            ZwiftAppToCompanion packetData;

            try
            {
                packetData = ZwiftAppToCompanion.Parser.ParseFrom(payload);
            }
            catch (InvalidProtocolBufferException ex)
            {
                _monitoringEvents.Warning("Invalid protobuf message: {Message}", ex.Message);
                return;
            }

            try
            {
                DecodeIncomingCore(packetData);
            }
            catch (Exception e)
            {
                _monitoringEvents.Error(e, "Failed to decode incoming message");
            }
        }

        private void DecodeIncomingCore(ZwiftAppToCompanion packetData)
        {
            if (packetData.Items == null || !packetData.Items.Any())
            {
                // We need to capture the first one to send an initialization message
                // back to Zwift so that it will start feeding us the actual data.
                OnZwiftPing(packetData);
                
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

                        OnCommandAvailable(buttonMessage.TypeId, buttonMessage.Title, buttonMessage.Tag1);

                        break;
                    case 9:
                        _monitoringEvents.Information("Received a pairing status message");
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
                    break;
                case 20:
                    // Ignore, contains very little data and is similar to type 21
                    break;
                case 21:
                    // Ignore, contains very little data
                    break;
            }
        }

        private void OnZwiftPing(ZwiftAppToCompanion zwiftAppToCompanion)
        {
            Enqueue(new ZwiftPingMessage
            {
                RiderId = zwiftAppToCompanion.RiderId
            });
        }

        private void OnActivityDetails(ulong activityId)
        {
            Enqueue(new ZwiftActivityDetailsMessage
            {
                ActivityId = activityId
            });
        }

        private void OnPowerUp(string type)
        {
            Enqueue(new ZwiftPowerUpMessage { Type = type });
        }

        private void OnCommandAvailable(uint numericalCommandType, string description, ulong sequenceNumber)
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

            Enqueue(new ZwiftCommandAvailableMessage
            {
                Type = commandType.ToString(),
                SequenceNumber = sequenceNumber
            });
        }

        private void OnRiderPosition(float latitude, float longitude, float altitude)
        {
            Enqueue(new ZwiftRiderPositionMessage
            {
                Latitude = latitude,
                Longitude = longitude,
                Altitude = altitude
            });
        }

        private void Enqueue(ZwiftMessage message)
        {
            try
            {
                _queue.Enqueue(message);
                
                if (_configuration.ThrottleMessages && 
                    _queue.Count > _configuration.MessageThrottleHighWaterMark)
                {
                    _autoResetEvent.Set();

                    _throttleResetEvent.WaitOne();
                }
            }
            catch (Exception e)
            {
                _monitoringEvents.Error(e, "Failed to enqueue message");
            }
            finally
            {
                // Unblock the Dequeue method
                _autoResetEvent.Set();
            }
        }

        public ZwiftMessage Dequeue(CancellationToken token)
        {
            do
            {
                try
                {
                    if (_queue.TryDequeue(out var message))
                    {
                        return message;
                    }
                    
                    _throttleResetEvent.Set();
                }
                catch (Exception e)
                {
                    _monitoringEvents.Error(e, "Failed to dequeue message");
                }

                _autoResetEvent.WaitOne(_queueWaitTimeout);
            } while (!token.IsCancellationRequested);
            
            return null;
        }
    }
}
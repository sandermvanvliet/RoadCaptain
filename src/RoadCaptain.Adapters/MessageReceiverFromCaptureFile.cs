using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PacketDotNet;
using RoadCaptain.Ports;
using SharpPcap;
using SharpPcap.LibPcap;

namespace RoadCaptain.Adapters
{
    internal class MessageReceiverFromCaptureFile : IMessageReceiver
    {
        private const int PayloadHighWaterMark = 1000;

        /// <summary>
        ///     The default Zwift TCP data port used by Zwift Companion
        /// </summary>
        private const int ZwiftCompanionTcpPort = 21587;

        private readonly string _captureFilePath;
        private readonly PacketAssembler _companionPacketAssemblerPcToApp;
        private readonly AutoResetEvent _enqueueResetEvent;
        private readonly MonitoringEvents _monitoringEvents;
        private readonly ConcurrentQueue<byte[]> _payloads = new();
        private readonly AutoResetEvent _receiveQueueResetEvent;
        private readonly CancellationTokenSource _tokenSource = new();
        private CaptureFileReaderDevice _device;
        private Task<Task> _receiveTask;

        public MessageReceiverFromCaptureFile(string captureFilePath, MonitoringEvents monitoringEvents)
        {
            _captureFilePath = captureFilePath;
            _monitoringEvents = monitoringEvents;
            _receiveQueueResetEvent = new AutoResetEvent(false);
            _enqueueResetEvent = new AutoResetEvent(true);

            _companionPacketAssemblerPcToApp = new PacketAssembler(_monitoringEvents);
            _companionPacketAssemblerPcToApp.PayloadReady += (_, e) => EnqueueForReceive(e.Payload);
        }

        public byte[] ReceiveMessageBytes()
        {
            if (_receiveTask == null)
            {
                _receiveTask = Task.Factory.StartNew(() => StartCaptureFromFileAsync(_tokenSource.Token));
            }

            while (!_tokenSource.IsCancellationRequested)
            {
                if (_payloads.TryDequeue(out var message))
                {
                    if (_payloads.Count < PayloadHighWaterMark - 100)
                    {
                        // Unblock enqueueing of payloads
                        _enqueueResetEvent.Set();
                    }

                    return message;
                }

                _receiveQueueResetEvent.WaitOne(250);
            }

            return null;
        }

        public void Shutdown()
        {
            try
            {
                _tokenSource.Cancel();
            }
            catch (OperationCanceledException)
            {
                // If the task has already been canceled or throws because
                // it's cancelling then we can ignore this.
            }
        }

        public void SendMessageBytes(byte[] payload)
        {
            // Ignore for now, nobody is listening
        }

        public void SendInitialPairingMessage(uint riderId, uint sequenceNumber)
        {
            // Ignore for now, nobody is listening
        }

        public void SendTurnCommand(TurnDirection direction, ulong sequenceNumber)
        {
            // Ignore for now, nobody is listening
        }

        private void EnqueueForReceive(byte[] payload)
        {
            // Put the payload on a queue and signal ReceiveMessageBytes() that it can unblock
            _payloads.Enqueue(payload);
            _receiveQueueResetEvent.Set();
        }

        private async Task StartCaptureFromFileAsync(CancellationToken cancellationToken = default)
        {
            if (File.Exists(_captureFilePath))
            {
                _device = new CaptureFileReaderDevice(_captureFilePath);
            }
            else
            {
                throw new FileNotFoundException("Capture file not found", _captureFilePath);
            }

            // Open the device for capturing
            // ReSharper disable once RedundantArgumentDefaultValue
            _device.Open(DeviceModes.None);
            _device.Filter = $"tcp port {ZwiftCompanionTcpPort}";

            _device.OnPacketArrival += OnPacketArrival;

            // Start capture 'INFINTE' number of packets
            await Task.Factory.StartNew(() => { _device.Capture(); }, cancellationToken);
        }

        protected void OnPacketArrival(object sender, PacketCapture eventArgs)
        {
            // To prevent filling up the queue we check if we can enqueue.
            // The AutoResetEvent blocks us if the payload queue count
            // goes over the PAYLOAD_HIGH_WATER_MARK. Once it's in that
            // state we loop and block 250ms every time until signalled
            // by ReceiveMessageBytes() when the count goes below the
            // PAYLOAD_HIGH_WATER_MARK
            while (!_enqueueResetEvent.WaitOne(250))
            {
                if (_payloads.Count < PayloadHighWaterMark)
                {
                    break;
                }
            }

            try
            {
                var packet = Packet.ParsePacket(eventArgs.Device.LinkType, eventArgs.Data.ToArray());
                var tcpPacket = packet.Extract<TcpPacket>();

                // Only care about packets from the game to the app,
                // the app to game packets we're sending ourselves anyway.                
                if (tcpPacket != null && (tcpPacket.DestinationPort == ZwiftCompanionTcpPort || tcpPacket.SourcePort == ZwiftCompanionTcpPort))
                {
                    _companionPacketAssemblerPcToApp.Assemble(tcpPacket);
                }
            }
            catch (Exception exception)
            {
                _monitoringEvents.Error(exception, "Unable to parse packet");
            }
            finally
            {
                _enqueueResetEvent.Set();
            }
        }
    }
}
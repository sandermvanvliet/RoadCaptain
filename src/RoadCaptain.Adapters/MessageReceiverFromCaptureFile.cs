using System;
using System.Collections.Generic;
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
        private readonly string _captureFilePath;
        private CaptureFileReaderDevice _device;
        private readonly MonitoringEvents _monitoringEvents;
        private DateTime? _offset;
        private readonly PacketAssembler _companionPacketAssemblerPcToApp;
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly AutoResetEvent _autoResetEvent;
        private readonly Queue<byte[]> _payloads = new();

        /// <summary>
        /// The default Zwift TCP data port used by Zwift Companion
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private const int ZWIFT_COMPANION_TCP_PORT = 21587;

        public MessageReceiverFromCaptureFile(string captureFilePath, MonitoringEvents monitoringEvents)
        {
            _captureFilePath = captureFilePath;
            _monitoringEvents = monitoringEvents;
            _autoResetEvent = new AutoResetEvent(false);

            _companionPacketAssemblerPcToApp = new PacketAssembler(_monitoringEvents);
            _companionPacketAssemblerPcToApp.PayloadReady += (_, e) => EnqueueForReceive(e.Payload);

            Task.Factory.StartNew(async () => await StartCaptureFromFileAsync(_tokenSource.Token));
        }

        private void EnqueueForReceive(byte[] payload)
        {
            // Put the payload on a queue and signal ReceiveMessageBytes() that it can unblock
            _payloads.Enqueue(payload);
            _autoResetEvent.Set();
        }

        private async Task StartCaptureFromFileAsync(CancellationToken cancellationToken = default)
        {
            if(File.Exists(_captureFilePath))
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
            _device.Filter = $"tcp port {ZWIFT_COMPANION_TCP_PORT}";

            _device.OnPacketArrival += OnPacketArrival;
            
            // Start capture 'INFINTE' number of packets
            await Task.Run(() => { _device.Capture(); }, cancellationToken);
        }

        protected void OnPacketArrival(object sender, PacketCapture eventArgs)
        {
            try
            {
                var packet = Packet.ParsePacket(eventArgs.Device.LinkType, eventArgs.Data.ToArray());
                var tcpPacket = packet.Extract<TcpPacket>();
                
                if (tcpPacket != null)
                {
                    _offset ??= eventArgs.Header.Timeval.Date;

                    tcpPacket.SequenceNumber = (uint)(eventArgs.Header.Timeval.Date - _offset.Value).TotalSeconds;
                    
                    int dstPort = tcpPacket.DestinationPort;

                    // Only care about packets from the game to the app,
                    // the app to game packets we're sending ourselves anyway.
                    if (dstPort == ZWIFT_COMPANION_TCP_PORT)
                    {
                        _companionPacketAssemblerPcToApp.Assemble(tcpPacket);
                    }
                }
            }
            catch (Exception exception)
            {
                _monitoringEvents.Error(exception, "Unable to parse packet");
            }
        }

        public byte[] ReceiveMessageBytes()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                if (_payloads.TryDequeue(out var message))
                {
                    return message;
                }

                _autoResetEvent.WaitOne(250);
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
            // Ignore for now
        }

        public void SendInitialPairingMessage(uint riderId)
        {
            // Ignore for now
        }
    }
}

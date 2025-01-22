// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Linq;
using PacketDotNet;

namespace RoadCaptain.Adapters.CaptureFile
{
    internal class DirectionalAssembler
    {
        private readonly MonitoringEvents _monitoringEvents;
        public event EventHandler<PayloadReadyEventArgs>? PayloadReady;

        public DirectionalAssembler(MonitoringEvents monitoringEvents)
        {
            _monitoringEvents = monitoringEvents;
        }
        private void OnPayloadReady(PayloadReadyEventArgs e)
        {
            var handler = PayloadReady;

            if (handler != null)
            {
                try
                {   
                    handler(this, e);
                }
                catch
                {
                    // Don't let downstream exceptions bubble up
                }
            }
        }
        private int _assembledLen;
        private byte[]? _payload;
        private bool _complete;
        private uint _startingSequenceNumber;

        /// <summary>
        /// Processes the current packet. If this packet is part of a fragmented sequence,
        /// its payload will be added to the internal buffer until the entire sequence payload has
        /// been loaded. When the packet sequence has been fully loaded, the <c>PayloadReady</c> event is invoked.
        /// </summary>
        /// <param name="packet">The packet to process</param>
        public void Assemble(TcpPacket packet)
        {
            packet = packet ?? throw new ArgumentNullException(nameof(packet));

            try
            {
                if (packet.Push && packet.Acknowledgment && _payload == null)
                {
                    // No reassembly required
                    _payload = packet.PayloadData;
                    _assembledLen = packet.PayloadData.Length;
                    _complete = true;
                    _startingSequenceNumber = packet.SequenceNumber;

                    Debug($"Complete packet - Actual: {_payload.Length}, Push: {packet.Push}, Ack: {packet.Acknowledgment}");
                }
                else if (packet.Push && packet.Acknowledgment)
                {
                    // Last packet in the sequence
                    _payload = _payload!.Concat(packet.PayloadData).ToArray();
                    _assembledLen += packet.PayloadData.Length;
                    _complete = true;

                    Debug($"Fragmented sequence finished - Actual: {_payload.Length}, Push: {packet.Push}, Ack: {packet.Acknowledgment}");
                }
                else if (packet.Acknowledgment && _payload == null)
                {
                    // First packet in a sequence
                    _payload = packet.PayloadData;
                    _assembledLen = packet.PayloadData.Length;
                    _startingSequenceNumber = packet.SequenceNumber;

                    Debug($"Fragmented packet started - Actual: {_payload.Length}, Push: {packet.Push}, Ack: {packet.Acknowledgment}");
                }
                else if (packet.Acknowledgment)
                {
                    // Middle packet in a sequence
                    _payload = _payload!.Concat(packet.PayloadData).ToArray();
                    _assembledLen += packet.PayloadData.Length;

                    Debug($"Fragmented packet continued - Actual: {_payload.Length}, Push: {packet.Push}, Ack: {packet.Acknowledgment}");
                }

                if (_complete && _payload?.Length > 0)
                {
                    Debug($"Packet completed!, Actual: {_assembledLen}, Push: {packet.Push}, Ack: {packet.Acknowledgment}");

                    OnPayloadReady(new PayloadReadyEventArgs(_startingSequenceNumber, _payload, false));

                    Reset();
                }
            }
            catch
            {
                Error("Failed to assemble packets");
                Reset();
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private void Debug(string message)
        {
            //_monitoringEvents.Debug(message);
        }

        private void Error(string message)
        {
            _monitoringEvents.Error(message);
        }

        /// <summary>
        /// Resets the internal state to start over
        /// </summary>
        public void Reset()
        {
            _payload = null;
            _assembledLen = 0;
            _complete = false;
            _startingSequenceNumber = 0;
        }
    }
}

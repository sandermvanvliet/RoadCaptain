using System;
using System.Collections.Generic;
using System.Linq;
using PacketDotNet;

namespace RoadCaptain.Adapters
{
    /// <summary>
    /// This helper class is used to identify and reassemble fragmented TCP payloads.
    /// 
    /// Many thanks to @jeroni7100 for figuring out the packet reassembly magic!
    /// </summary>
    internal class PacketAssembler
    {
        public event EventHandler<PayloadReadyEventArgs> PayloadReady;

        private byte[] _payload;
        private bool _complete;
        private uint _startingSequenceNumber;
        private readonly MonitoringEvents _monitoringEvents;
        private ulong _expectedNextSequenceNumber;
        private readonly List<TcpPacket> _packetBuffer = new();
        private bool _lastOnlyAck;
        private ulong _lastSequenceNumber;

        public PacketAssembler(MonitoringEvents monitoringEvents)
        {
            _monitoringEvents = monitoringEvents;
        }

        private void OnPayloadReady(PayloadReadyEventArgs e)
        {
            EventHandler<PayloadReadyEventArgs> handler = PayloadReady;
            if (handler != null)
            {
                try {
                    handler(this, e);
                }
                catch {
                    // Don't let downstream exceptions bubble up
                }
            }
        } 

        public void Assemble(TcpPacket packet)
        {
            packet = packet ?? throw new ArgumentException(nameof(packet));

            if (packet.Synchronize || (packet.Synchronize && packet.Acknowledgment))
            {
                _lastSequenceNumber = 0;
                _expectedNextSequenceNumber = 0;
                _startingSequenceNumber = 0;
                return;
            }

            if (packet.Reset || packet.Finished)
            {
                return;
            }

            if (_expectedNextSequenceNumber == 0 || packet.SequenceNumber == _expectedNextSequenceNumber)
            {
                InnerAssemble(packet);
                _expectedNextSequenceNumber = packet.SequenceNumber + (uint)packet.PayloadData.Length;
                _lastSequenceNumber = packet.SequenceNumber;

                _lastOnlyAck = packet.Acknowledgment && !packet.Push;
            }
            else if (packet.SequenceNumber == _lastSequenceNumber && _lastOnlyAck)
            {
                if (packet.PayloadData.Length > 0)
                {
                    InnerAssemble(packet);
                    _expectedNextSequenceNumber = packet.SequenceNumber + (uint)(packet.PayloadData.Length);
                    _lastSequenceNumber = packet.SequenceNumber;

                    _lastOnlyAck = packet.Acknowledgment && !packet.Push;
                }
            }
            else if (packet.SequenceNumber > _expectedNextSequenceNumber)
            {
                if (_packetBuffer.Any(p => p.SequenceNumber == packet.SequenceNumber))
                {
                    _monitoringEvents.Warning("Packet {SequenceNumber} has been seen before, not buffering it again", packet.SequenceNumber);
                    return;
                }

                _monitoringEvents.Warning("Received a packet to far ahead, buffering {SequenceNumber}, expected {ExpectedSequenceNumber}", packet.SequenceNumber, _expectedNextSequenceNumber);
                _packetBuffer.Add(packet);

                var expectedPacket =
                    _packetBuffer.SingleOrDefault(p => p.SequenceNumber == _expectedNextSequenceNumber);

                while (expectedPacket != null)
                {
                    _monitoringEvents.Information("Unbuffering {SequenceNumber}", expectedPacket.SequenceNumber);

                    _packetBuffer.Remove(expectedPacket);

                    Assemble(expectedPacket);

                    expectedPacket =
                        _packetBuffer.SingleOrDefault(p => p.SequenceNumber == _expectedNextSequenceNumber);
                }
            }
            else
            {
                _monitoringEvents.Warning("Received a packet with a sequence number {SequenceNumber} that we've already passed ({LastSequenceNumber}) Skipping it because it's (most likely) a TCP retransmission.", packet.SequenceNumber, _lastSequenceNumber);
            }
        } 

        /// <summary>
        /// Processes the current packet. If this packet is part of a fragmented sequence,
        /// its payload will be added to the internal buffer until the entire sequence payload has
        /// been loaded. When the packet sequence has been fully loaded, the <c>PayloadReady</c> event is invoked.
        /// </summary>
        /// <param name="packet">The packet to process</param>
        private void InnerAssemble(TcpPacket packet)
        {
            packet = packet ?? throw new ArgumentNullException(nameof(packet));
            
            try
            {
                if (packet.Push && packet.Acknowledgment && _payload == null)
                {
                    // No reassembly required
                    _payload = packet.PayloadData;
                    _complete = true;
                    _startingSequenceNumber = packet.SequenceNumber;
                }
                else if (packet.Push && packet.Acknowledgment)
                {
                    // Last packet in the sequence
                    _payload = _payload.Concat(packet.PayloadData).ToArray();
                    _complete = true;
                }
                else if (packet.Acknowledgment && _payload == null)
                {
                    // First packet in a sequence
                    _payload = packet.PayloadData;
                    _startingSequenceNumber = packet.SequenceNumber;
                }
                else if (packet.Acknowledgment) {
                    // Middle packet in a sequence
                    _payload = _payload.Concat(packet.PayloadData).ToArray();
                }

                if (_complete && _payload?.Length > 0)
                {
                    OnPayloadReady(new PayloadReadyEventArgs
                    {
                        Payload = _payload,
                        SequenceNumber = _startingSequenceNumber
                    });

                    Reset();
                }
            }
            catch (Exception ex)
            {
                _monitoringEvents.Error(ex, "Failed to assemble packets");
                Reset();
            }
        }

        /// <summary>
        /// Resets the internal state to start over
        /// </summary>
        public void Reset()
        {
            _payload = null;
            _complete = false;
            _startingSequenceNumber = 0;
        }
    }
}
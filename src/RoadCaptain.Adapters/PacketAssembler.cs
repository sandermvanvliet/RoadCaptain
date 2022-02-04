using System;
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
        
        private int _assembledLen;
        private byte[] _payload;
        private bool _complete;
        private uint _startingSequenceNumber;
        private readonly MonitoringEvents _monitoringEvents;

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

        /// <summary>
        /// Processes the current packet. If this packet is part of a fragmented sequence,
        /// its payload will be added to the internal buffer until the entire sequence payload has
        /// been loaded. When the packet sequence has been fully loaded, the <c>PayloadReady</c> event is invoked.
        /// </summary>
        /// <param name="packet">The packet to process</param>
        public void Assemble(TcpPacket packet)
        {
            packet = packet ?? throw new ArgumentNullException(nameof(packet));

            if (_startingSequenceNumber == 0)
            {
                _startingSequenceNumber = packet.SequenceNumber;
            }

            try
            {
                if (packet.Push && packet.Acknowledgment && _payload == null)
                {
                    // No reassembly required
                    _payload = packet.PayloadData;
                    _assembledLen = packet.PayloadData.Length;
                    _complete = true;
                }
                else if (packet.Push && packet.Acknowledgment)
                {
                    // Last packet in the sequence
                    _payload = _payload.Concat(packet.PayloadData).ToArray();
                    _assembledLen += packet.PayloadData.Length;
                    _complete = true;
                }
                else if (packet.Acknowledgment && _payload == null)
                {
                    // First packet in a sequence
                    _payload = packet.PayloadData;
                    _assembledLen = packet.PayloadData.Length;
                }
                else if (packet.Acknowledgment) {
                    // Middle packet in a sequence
                    _payload = _payload.Concat(packet.PayloadData).ToArray();
                    _assembledLen += packet.PayloadData.Length;
                }

                if (_complete && _payload?.Length > 0)
                {
                    // Break apart any concatenated payloads
                    var offset = 0;

                    while (offset < _assembledLen)
                    {
                        var length = ToUInt16(_payload, offset, 2);

                        if (offset + length < _assembledLen)
                        {
                            var payload = _payload.Skip(offset).Take(length + 2).ToArray();

                            if (payload.Length > 0)
                            {
                                OnPayloadReady(new PayloadReadyEventArgs
                                {
                                    Payload = payload, 
                                    SequenceNumber = _startingSequenceNumber
                                });
                            }
                        }

                        offset += 2 + length;
                    }

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
            _assembledLen = 0;
            _complete = false;
            _startingSequenceNumber = 0;
        }

        private static int ToUInt16(byte[] buffer, int start, int count)
        {
            if (buffer.Length > 2)
            {
                var b = buffer.Skip(start).Take(count).ToArray();
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(b);
                }

                if (b.Length == count)
                {
                    return (BitConverter.ToUInt16(b, 0));
                }

                return 0;
            }

            return 0;
        }
    }
}
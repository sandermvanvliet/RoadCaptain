// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PacketDotNet;

namespace RoadCaptain.Adapters.CaptureFile
{
    /// <summary>
    /// This helper class is used to identify and reassemble fragmented TCP payloads.
    /// 
    /// Many thanks to @jeroni7100 for figuring out the packet reassembly magic!
    /// </summary>
    internal class PacketAssembler
    {
        public event EventHandler<PayloadReadyEventArgs> PayloadReady;
        
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

        private readonly List<uint> _pendingClientAcks = new();
        private readonly List<uint> _pendingServerAcks = new();
        
        private bool _handshakeComplete;
        private int _handshakeStep;
        private readonly DirectionalAssembler _clientToServerAssembler;
        private readonly DirectionalAssembler _serverToClientAssembler;
        private bool _closing;
        private readonly MonitoringEvents _monitoringEvents;

        public PacketAssembler(MonitoringEvents monitoringEvents)
        {
            _monitoringEvents = monitoringEvents;
            _serverToClientAssembler = new(monitoringEvents);
            _clientToServerAssembler = new(monitoringEvents);
            _clientToServerAssembler.PayloadReady += (_, args) =>
            {
                args.ClientToServer = true;
                OnPayloadReady(args);
            };
            _serverToClientAssembler.PayloadReady += (_, args) =>
            {
                args.ClientToServer = false;
                OnPayloadReady(args);
            };
        }

        public void Assemble(TcpPacket packet)
        {
            packet = packet ?? throw new ArgumentException(nameof(packet));

            if (!_handshakeComplete)
            {
                if (_closing)
                {
                    if (packet.Synchronize)
                    {
                        // Starting a new connection
                        _closing = false;
                    }
                    else if (packet.Acknowledgment || 
                        (packet.Finished && packet.Acknowledgment) ||
                        packet.Reset)
                    {
                        return;
                    }
                }

                // If the connection isn't closing and the handshake
                // is not complete then we treat a RST as a true reset
                // and re-initialize the handshake
                if (packet.Reset)
                {
                    _handshakeStep = 0;
                    return;
                }

                PerformHandshake(packet);

                if (_handshakeComplete)
                {
                    // Add the last sequence number because
                    // we can get a TCP window update with 
                    // that ACK number
                    _pendingServerAcks.Add(packet.SequenceNumber);
                }

                return;
            }

            if (packet.Finished)
            {
                // Connection force closed, reset everything
                _handshakeComplete = false;
                _handshakeStep = 0;
                _closing = true;

                // If the TCP packet has the PSH flag set that
                // means that there is still data in the packet
                // that we should capture.
                if (!packet.Push)
                {
                    return;
                }
            }
            if (packet.Reset)
            {
                // Connection force closed, reset everything
                _handshakeComplete = false;
                _handshakeStep = 0;
                _pendingClientAcks.Clear();
                _pendingServerAcks.Clear();
                return;
            }

            Debug($"{(packet.DestinationPort == 21587 ? "C -> S" : "S -> C")} {packet.SequenceNumber,14} {packet.AcknowledgmentNumber,14} {(packet.Synchronize ? "SYN " : "")}{(packet.Push ? "PSH " : "")}{(packet.Acknowledgment ? "ACK " : "")}");

            if (packet.DestinationPort == 21587)
            {
                if (_pendingClientAcks.Any() && !_pendingClientAcks.Contains(packet.AcknowledgmentNumber))
                {
                    Error($"{packet.AcknowledgmentNumber} was not expected from the client");
                    
                    Debugger.Break();
                }
                else
                {
                    _pendingServerAcks.Add(packet.SequenceNumber + PayloadDataLength(packet));

                    _clientToServerAssembler.Assemble(packet);
                }
            }
            else if (packet.SourcePort == 21587)
            {
                // Server-to-client
                if (!_pendingServerAcks.Contains(packet.AcknowledgmentNumber))
                {
                    Error($"{packet.AcknowledgmentNumber} was not expected from the server");
                    Debugger.Break();
                }
                else
                {
                    _pendingClientAcks.Add(packet.SequenceNumber + PayloadDataLength(packet));
                    
                    _serverToClientAssembler.Assemble(packet);
                }
            }
        }

        private void PerformHandshake(TcpPacket packet)
        {
            if (packet.Synchronize && !packet.Acknowledgment)
            {
                _handshakeStep = 1;
            }
            else if (packet.Synchronize && packet.Acknowledgment && _handshakeStep == 1)
            {
                _handshakeStep = 2;
            }
            else if (_handshakeStep == 2 && packet.Acknowledgment)
            {
                _handshakeStep = 3;
            }
            else
            {
                Error($"Handshake failed at step {_handshakeStep} with sequence no {packet.SequenceNumber}");
            }

            if (_handshakeStep == 3)
            {
                Info("Handshake complete");
                _handshakeComplete = true;
                _closing = false;
            }
        }

        private void Debug(string message)
        {
            //_monitoringEvents.Debug(message);
        }

        private void Info(string message)
        {
            _monitoringEvents.Information(message);
        }

        private void Error(string message)
        {
            _monitoringEvents.Error(message);
        }

        private static uint PayloadDataLength(TcpPacket packet)
        {
            return packet.HasPayloadData
                ? (uint)packet.PayloadData.Length
                : 0;
        }
    }
}

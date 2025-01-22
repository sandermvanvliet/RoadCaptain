// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Threading;

namespace RoadCaptain.Ports
{
    public interface IMessageEmitter
    {
        void EmitMessageFromBytes(byte[] payload);
        ZwiftMessage? Dequeue(CancellationToken token);
    }
}

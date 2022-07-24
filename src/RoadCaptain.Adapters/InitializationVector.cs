using System;

namespace RoadCaptain.Adapters
{
    internal class InitializationVector
    {
        private readonly byte[] _vector;

        public InitializationVector(ChannelType channelType)
        {
            _vector = CreateInitializationVector(channelType);
        }

        private static byte[] CreateInitializationVector(ChannelType channelType)
        {
            var deviceTypeBytes = BitConverter.GetBytes((short)2); // 2 = device type Zwift Companion (Zc)
            var channelTypeBytes = BitConverter.GetBytes((short)channelType);
            var connectionIdBytes = BitConverter.GetBytes(0);
            
            // Note that the bytes are written "reversed"
            // because the data is expected in Big Endian 
            // order.
            return new byte[]
            {
                0,
                0,
                deviceTypeBytes[1],
                deviceTypeBytes[0],
                channelTypeBytes[1],
                channelTypeBytes[0],
                connectionIdBytes[1],
                connectionIdBytes[0],
                0,
                0,
                0,
                0
            };
        }

        private int BABitTwiddle(short value)
        {
            return value & 65535;
        }

        public int GetConnectionId()
        {
            var connectionIdBytes = new[] { _vector[7], _vector[6] };

            return BABitTwiddle(BitConverter.ToInt16(connectionIdBytes));
        }

        public void SetCounter(long value)
        {
            var bytes = BitConverter.GetBytes((int)value); // Yes, cast to int because why not...
            Array.Reverse(bytes);
            bytes.CopyTo(_vector, 8);
        }

        public void IncrementCounter()
        {
            var value = GetCounter() + 1;

            SetCounter(value);
        }

        public int GetCounter()
        {
            var bytes = new[] { _vector[11], _vector[10], _vector[9], _vector[8] };
            return BitConverter.ToInt32(bytes);
        }

        public byte[] GetBytes()
        {
            return _vector;
        }

        public void SetConnectionId(short value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            bytes.CopyTo(_vector, 6);
        }
    }
}
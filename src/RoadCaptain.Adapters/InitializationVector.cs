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
            var deviceTypeBytes = BitConverter.GetBytes((short)2);
            Array.Reverse(deviceTypeBytes); // Because Endianness
            var channelTypeBytes = BitConverter.GetBytes((short)channelType);
            Array.Reverse(channelTypeBytes); // Because Endianness
            var connectionIdBytes = BitConverter.GetBytes(0);
            
            return new byte[]
            {
                0,
                0,
                deviceTypeBytes[0],
                deviceTypeBytes[1],
                channelTypeBytes[0],
                channelTypeBytes[1],
                connectionIdBytes[0],
                connectionIdBytes[1],
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

        public int getConnectionId()
        {
            var connectionIdBytes = new[] { _vector[7], _vector[6] };

            return BABitTwiddle(BitConverter.ToInt16(connectionIdBytes));
        }

        public void setCounter(long value)
        {
            var bytes = BitConverter.GetBytes((int)value); // Yes, cast to int because why not...
            Array.Reverse(bytes);
            bytes.CopyTo(_vector, 8);
        }

        public void incrementCounter()
        {
            var value = getCounter() + 1;

            setCounter(value);
        }

        public int getCounter()
        {
            var bytes = new[] { _vector[11], _vector[10], _vector[9], _vector[8] };
            return BitConverter.ToInt32(bytes);
        }

        public static implicit operator byte[](InitializationVector self)
        {
            return self._vector;
        }

        public void setConnectionId(short value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            bytes.CopyTo(_vector, 6);
        }
    }
}
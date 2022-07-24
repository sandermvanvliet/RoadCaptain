using System;
using System.IO;

namespace RoadCaptain.Adapters
{
    internal class ByteBuffer
    {
        private readonly byte[] _data;
        private readonly MemoryStream _stream;

        public ByteBuffer(byte[] data)
        {
            _data = data;
            _stream = new MemoryStream(_data);
            Limit = _stream.Length;
        }

        public int Position
        {
            get => (int)_stream.Position;
            set => _stream.Seek(value, SeekOrigin.Begin);
        }

        public long Limit { get; set; }

        public int Remaining => (int)(Limit - _stream.Position);

        public byte GetByte()
        {
            return (byte)_stream.ReadByte();
        }

        public int GetInt()
        {
            var intBuffer = new byte[4];
            _stream.Read(intBuffer, 0, 4);
            Array.Reverse(intBuffer);
            return BitConverter.ToInt32(intBuffer);
        }

        public short GetShort()
        {
            var intBuffer = new byte[2];
            _stream.Read(intBuffer, 0, 2);
            Array.Reverse(intBuffer);
            return BitConverter.ToInt16(intBuffer);
        }

        public static ByteBuffer Allocate(int size)
        {
            return new ByteBuffer(new byte[size]);
        }

        public void Put(ByteBuffer input)
        {
            for (var i = input.Position; i < input.Limit; i++)
            {
                var value = input.GetByte();
                _stream.WriteByte(value);
            }
        }

        public void Put(byte value)
        {
            _stream.WriteByte(value);
        }

        public void Flip()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            Limit = _stream.Length;
        }

        public static implicit operator byte[]?(ByteBuffer? self)
        {
            return self?._data;
        }

        public byte[] ToArray()
        {
            var result = new byte[Remaining];
            _data.AsSpan().Slice(Position, Remaining).CopyTo(result);
            return result;
        }

        public void PutInt(int relayId)
        {
            var bytes = BitConverter.GetBytes(relayId);
            Array.Reverse(bytes);
            _stream.Write(bytes, 0, 4);
        }

        public void PutShort(short value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            _stream.Write(bytes, 0, 2);
        }
    }
}
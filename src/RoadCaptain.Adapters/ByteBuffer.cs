using System;
using System.IO;

namespace RoadCaptain.Adapters
{
    public class ByteBuffer
    {
        private readonly byte[] _data;
        private readonly MemoryStream _stream;
        private long _limit;

        public ByteBuffer(byte[] data)
        {
            _data = data;
            _stream = new MemoryStream(_data);
            _limit = _stream.Length;
        }

        public int remaining()
        {
            return (int)(_limit - _stream.Position);
        }

        public byte get()
        {
            return (byte)_stream.ReadByte();
        }

        public int position()
        {
            return (int)_stream.Position;
        }

        public int position(int position)
        {
            return (int)_stream.Seek(position, SeekOrigin.Begin);
        }

        public int getInt()
        {
            var intBuffer = new byte[4];
            _stream.Read(intBuffer, 0, 4);
            Array.Reverse(intBuffer);
            return BitConverter.ToInt32(intBuffer);
        }

        public short getShort()
        {
            var intBuffer = new byte[2];
            _stream.Read(intBuffer, 0, 2);
            Array.Reverse(intBuffer);
            return BitConverter.ToInt16(intBuffer);
        }

        public int limit()
        {
            return (int)_limit;
        }

        public void limit(long value)
        {
            _limit = value;
        }

        public static ByteBuffer allocate(int size)
        {
            return new ByteBuffer(new byte[size]);
        }

        public void put(ByteBuffer input)
        {
            for (var i = input.position(); i < input._limit; i++)
            {
                var value = input.get();
                _stream.WriteByte(value);
            }
        }

        public void flip()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            _limit = _stream.Length;
        }

        public static implicit operator byte[]?(ByteBuffer? self)
        {
            return self?._data;
        }

        public long Position => position();
        public long Limit => limit();
        public long Size => _stream.Length;
        public long Remaining => remaining();

        public byte[] array()
        {
            var result = new byte[Remaining];
            _data.AsSpan().Slice((int)Position, (int)Remaining).CopyTo(result);
            return result;
        }
    }
}
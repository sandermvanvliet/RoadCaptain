using System;

namespace RoadCaptain.Adapters
{
    public class DataEventArgs : EventArgs
    {
        public byte[] Data { get; }

        public DataEventArgs(byte[] data)
        {
            Data = data;
        }
    }
}
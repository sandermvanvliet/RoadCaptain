using System;

namespace RoadCaptain
{
    public class CryptographyException : Exception
    {
        public CryptographyException(Exception inner)
            : base((string?)inner.Message, (Exception?)inner)
        {
        }

        public CryptographyException(string message)
            : base(message)
        {   
        }
    }
}
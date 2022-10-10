// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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

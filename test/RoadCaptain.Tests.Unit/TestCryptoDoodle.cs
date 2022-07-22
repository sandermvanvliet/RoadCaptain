using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using RoadCaptain.Adapters;
using Xunit;

namespace RoadCaptain.Tests.Unit
{
    public class TestCryptoDoodle
    {
        [Fact]
        public void EncryptionTest()
        {
            var key = Convert.FromBase64String("3qlTEMUx3dZ86aH2kHavUA==");
            var messageBytes = Convert.FromBase64String("AgAAa4Tlae6h+nJoNatW2A2jIOWov0YZ");

            var doodle = new ZwiftCrypto(new TestUserPreferences { ConnectionSecret = key });

            var decrypted = doodle.Decrypt(new ByteBuffer(messageBytes));

            decrypted
                .Should()
                .NotBeNullOrEmpty();

        }

        private byte[] FromWireShark(string input)
        {
            var retval = new List<byte>();

            for (var index = 0; index < input.Length; index += 2)
            {
                var toDecode = input[index] + "" + input[index + 1];

                retval.AddRange(BitConverter.GetBytes(Int16.Parse(toDecode, NumberStyles.HexNumber)));
            }

            return retval.ToArray();
        }
    }
}

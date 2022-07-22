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

            var doodle = new ZwiftCrypto(new TestUserPreferences { ConnectionSecret = key }, new NopMonitoringEvents());

            Action action = () => doodle.Decrypt(new ByteBuffer(messageBytes));

            action
                .Should()
                .NotThrow();
        }

        [Fact]
        public void EncryptionTestRoundtrip()
        {
            var key = Convert.FromBase64String("3qlTEMUx3dZ86aH2kHavUA==");
            var messageBytes = Convert.FromBase64String("AgAAa4Tlae6h+nJoNatW2A2jIOWov0YZ");

            var doodle = new ZwiftCrypto(new TestUserPreferences { ConnectionSecret = key }, new NopMonitoringEvents());

            byte[]? decrypted = null;

            Action action = () => decrypted = doodle.Decrypt(new ByteBuffer(messageBytes));

            action
                .Should()
                .NotThrow();

            action = () => doodle.Encrypt(decrypted);

            action
                .Should()
                .NotThrow();
        }

        [Fact]
        public void EncryptionTestRepro1()
        {
            var key = Convert.FromBase64String("Xsnlr1vNWfryLM0Qf9FGxQ==");
            var messageBytes = Convert.FromBase64String("AgAAquAFg5vyK5iGSL/Vgxu8G3H1hnE4");

            var doodle = new ZwiftCrypto(new TestUserPreferences { ConnectionSecret = key }, new NopMonitoringEvents());

            Action action = () => doodle.Decrypt(new ByteBuffer(messageBytes));

            action
                .Should()
                .NotThrow();
        }

        [Fact]
        public void EncryptionTestRepro2()
        {
            var key = Convert.FromBase64String("c5Lg9jWVfa3gOafUoDmn5w==");
            var messageBytes = Convert.FromBase64String("ABbQWYyo8DX8pCfdZ3uRfEiyZXudmQ==");

            var doodle = new ZwiftCrypto(new TestUserPreferences { ConnectionSecret = key }, new NopMonitoringEvents());

            Action action = () => doodle.Decrypt(new ByteBuffer(messageBytes));

            action
                .Should()
                .NotThrow();
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

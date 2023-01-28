// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.Ports;
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

            Action action = () => doodle.Decrypt(new ByteBuffer(messageBytes).ToArray());

            action
                .Should()
                .NotThrow();
        }

        [Fact]
        public void EncryptionTestRoundtrip()
        {
            var key = Convert.FromBase64String("3qlTEMUx3dZ86aH2kHavUA==");
            var messageBytes = Convert.FromBase64String("AgAAa4Tlae6h+nJoNatW2A2jIOWov0YZ");

            var doodle = new ZwiftCrypto(new TestUserPreferences { ConnectionSecret = key });

            byte[]? decrypted = null;

            Action action = () =>
            {
                var decryptionResult = doodle.Decrypt(new ByteBuffer(messageBytes).ToArray()) as SuccessfulDecryptionResult;
                decrypted = decryptionResult.Data;
            };

            action
                .Should()
                .NotThrow();

            action = () => doodle.Encrypt(decrypted);

            action
                .Should()
                .NotThrow();
        }
    }
}


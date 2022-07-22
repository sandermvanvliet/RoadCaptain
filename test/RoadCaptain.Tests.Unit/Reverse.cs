using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Crypto.Utilities;
using Xunit;

namespace RoadCaptain.Tests.Unit
{
    public class Reverse
    {
        [Fact]
        public void Dwim()
        {
            var result = A(new byte[]{62, 113, 116, 107, 106, 60, 121, 8, 10, 66, 47, 85, 67, 32, 68, 17, 83});

            Debugger.Break();
        }

        [Fact]
        public void KeyGen()
        {
            var aes = Aes.Create();
            aes.KeySize = 128;

            aes.GenerateKey();
            var connectionSecret = Convert.ToBase64String(aes.Key);

            Debugger.Break();
        }

        [Fact]
        public void EncryptRoundTrip()
        {
            var encryptAes = Aes.Create();
            encryptAes.KeySize = 128;
            encryptAes.BlockSize = 128;
            //encryptAes.GenerateIV();
            encryptAes.GenerateKey();
            var connectionSecret = Convert.ToBase64String(encryptAes.Key);

            var plainText = "SUPERSECRET";
            var plainTextBytes = Encoding.ASCII.GetBytes(plainText);
            //var initializationVector = new byte[] { /* device type == Zc == 2 */ 2, /* channel type == TcpClient == 3 */ 3, 2};
            var initializationVector = GenerateInitializationVector(/* device type == Zc == 2 */ 2, /* channel type == TcpClient == 3 */ 3, 0 /* hardcoded thingy */);
            var encryptedData = encryptAes.EncryptCbc(plainTextBytes, initializationVector, PaddingMode.PKCS7);

            var decrypAes = Aes.Create();
            decrypAes.KeySize = 128;
            decrypAes.Key = Convert.FromBase64String(connectionSecret);

            var decryptedBytes = decrypAes.DecryptEcb(encryptedData, PaddingMode.None);

            Encoding.ASCII.GetString(decryptedBytes)
                .Should()
                .Be(plainText);
        }

        private static byte[] GenerateInitializationVector(short deviceType, short channelType, int whatever)
        {
            var deviceTypeBytes = BitConverter.GetBytes(deviceType);
            var channelTypeBytes = BitConverter.GetBytes(channelType);
            var whateverBytes = BitConverter.GetBytes(whatever);
            
            return new byte[]
            {
                0,
                0,
                deviceTypeBytes[0],
                deviceTypeBytes[1],
                channelTypeBytes[0],
                channelTypeBytes[1],
                whateverBytes[0],
                whateverBytes[1],
                0,
                0,
                0,
                0
            };
        }

        [Fact]
        public void RoundTripBouncy()
        {
            var encryptAes = Aes.Create();
            encryptAes.KeySize = 128;
            encryptAes.BlockSize = 128;
            encryptAes.GenerateKey();

            var plainText = "SUPERSECRET";
            var cipherText = Encrypt(plainText, encryptAes.Key);

            var deciphered = Decrypt(cipherText, encryptAes.Key);

            var decipheredPlainText = Encoding.ASCII.GetString(deciphered);

            decipheredPlainText
                .Should()
                .Be(plainText);
        }

        private byte[] Decrypt(Span<byte> cipherText, byte[] key, byte[]? initializationVector = null)
        {
            initializationVector ??= GenerateInitializationVector(
                /* device type == Zc == 2 */2, 
                /* channel type == TcpClient == 3 */ 3, 
                /* hardcoded thingy, always zero */ 0 );

            var gcm = new AesGcm(key);
            var tagLength = 12;
            var tag = cipherText.Slice(0, tagLength);
            var toDecrypt = cipherText.Slice(tagLength);
            var decryptedData = new byte[toDecrypt.Length];
            var nonce = new byte[12];
            gcm.Decrypt(nonce, toDecrypt, tag, decryptedData, initializationVector);

            return decryptedData;   
        }

        [Fact]
        public void ZwiftDecrypt()
        {
            var messageBytes = FromWireShark(
                "02-00-01-52-7E-09-3D-BA-0A-B9-52-5D-7E-AF-71-8E-17-EE-7B-46-7E-A7-EB-B5".Replace("-", ""));
            messageBytes = FromWireShark("00000018020000c783311f30d6abdfaf1ed9729c9f7ebfd5aa15c024");
            var a = messageBytes[0];
            if ((a & 240) >> 4 == 0)
            {
                if (h(a))
                {
                    Debugger.Break();
                }

                if (g(a))
                {
                    if (messageBytes.Length - 1 >= 2)
                    {
                        var whatever = new[] { messageBytes[1], messageBytes[2] };

                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(whatever);
                        }

                        var getShort = BitConverter.ToUInt16(whatever);
                        if (getShort != 0)
                        {

                        }
                        Debugger.Break();
                    }
                }

                if (i(a))
                {
                    // Apparently not...
                }

                messageBytes = messageBytes.AsSpan().Slice(3, messageBytes.Length - 3 - 4).ToArray();
            }
            
            var key = Convert.FromBase64String("e2mQiH8JVxV98ZLhNpfnRQ==");
            var iv = GenerateInitializationVector(
                /* device type == Zc == 2 */2, 
                /* channel type == TcpServer == 4 */ 3, 
                /* hardcoded thingy, always zero */ 0 );
            var plainTextBytes = Decrypt(messageBytes.ToArray(), key, iv);

            plainTextBytes
                .Should()
                .NotBeNullOrEmpty();

            Debugger.Break();
        }

        private static bool h(int i) {
            return (i & 4) != 0;
        }

        private static bool g(int i) {
            return (i & 2) != 0;
        }

        private static bool i(int i) {
            return (i & 1) != 0;
        }




        private static int ToUInt16(ReadOnlySequence<byte> buffer, int start, int count)
        {
            if (buffer.Length >= start + count)
            {
                var b = buffer.Slice(start, count).ToArray();
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(b);
                }

                if (b.Length == count)
                {
                    return (BitConverter.ToUInt16(b, 0));
                }

                return 0;
            }

            return 0;
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

        private static byte[] Encrypt(string plainText, byte[] key)
        {
            var plainTextBytes = Encoding.ASCII.GetBytes(plainText);
            var initializationVector = GenerateInitializationVector(
                /* device type == Zc == 2 */2, 
                /* channel type == TcpClient == 3 */ 3, 
                /* hardcoded thingy, always zero */ 0 );

            var gcm = new AesGcm(key);
            var encryptedData = new byte[plainTextBytes.Length];
            var nonce = new byte[12];
            var tag = new byte[12];
            gcm.Encrypt(nonce, plainTextBytes, encryptedData, tag, initializationVector);

            return tag.Concat(encryptedData).ToArray();
        }

        private static byte[] a = {Byte.MaxValue, 52, 39, 68, 45};

        public static String A(byte[] bArr) {
            StringBuilder stringJoiner = new StringBuilder("");
            for (int i = 0; i < bArr.Length; i++) {
                byte b = bArr[i];
                byte[] bArr2 = a;
                stringJoiner.Append((char)(b ^ bArr2[i % bArr2.Length]));
            }
            return stringJoiner.ToString();
        }


    }
}

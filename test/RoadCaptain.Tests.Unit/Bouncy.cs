using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xunit;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace RoadCaptain.Tests.Unit
{
    public class Bouncy
    {
        [Fact]
        public void BounceEncrypt()
        {
            var crypto = new AesBcCrypto();

            // var input = "02-00-01-52-7E-09-3D-BA-0A-B9-52-5D-7E-AF-71-8E-17-EE-7B-46-7E-A7-EB-B5".Replace("-", "");
            //var input = "000000160080e3358577adbd50330f8129dc2b4ce8a91d2a5151";
            var input = "80e3358577adbd50330f8129dc2b4ce8a91d2a5151";

            crypto.Decrypt(
                Convert.ToBase64String(FromWireShark(input)),
                Convert.FromBase64String("e2mQiH8JVxV98ZLhNpfnRQ=="));
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

    public enum CipherMode
    {
        CBC,
        GCM
    }

    public enum Padding
    {
        NoPadding,
        PKCS7
    }

    public class AesBcCrypto
    {
        private const string ALGORITHM = "AES";

        private const byte AesIvSize = 16;
        private const byte GcmTagSize = 16; // in bytes

        private readonly CipherMode _cipherMode;
        private readonly Padding _padding;

        private readonly string _algorithm;

        public AesBcCrypto(CipherMode cipherMode = CipherMode.GCM, Padding padding = Padding.NoPadding)
        {
            _cipherMode = cipherMode;
            _padding = padding;
            _algorithm = $"{ALGORITHM}/{_cipherMode}/{_padding}";
        }

        public string Encrypt(string plainText, byte[] key)
        {
            var random = new SecureRandom();
            var iv = random.GenerateSeed(AesIvSize);
            var keyParameters = CreateKeyParameters(key, iv, GcmTagSize * 8);
            var cipher = CipherUtilities.GetCipher(_algorithm);
            cipher.Init(true, keyParameters);

            var plainTextData = Encoding.UTF8.GetBytes(plainText);
            var cipherText = cipher.DoFinal(plainTextData);

            return PackCipherData(cipherText, iv);
        }

        public string Decrypt(string cipherText, byte[] key)
        {
            var (encryptedBytes, iv, tagSize) = UnpackCipherData(cipherText);
            var keyParameters = CreateKeyParameters(key, iv, tagSize * 8);
            var cipher = CipherUtilities.GetCipher(_algorithm);
            cipher.Init(false, keyParameters);

            var decryptedData = cipher.DoFinal(encryptedBytes);
            return Encoding.UTF8.GetString(decryptedData);
        }

        private ICipherParameters CreateKeyParameters(byte[] key, byte[] iv, int macSize)
        {
            var keyParameter = new KeyParameter(key);
            if (_cipherMode == CipherMode.CBC)
            {
                return new ParametersWithIV(keyParameter, iv);
            }
            else if (_cipherMode == CipherMode.GCM)
            {
                return new AeadParameters(keyParameter, macSize, iv);
            }

            throw new Exception("Unsupported cipher mode");
        }

        private string PackCipherData(byte[] encryptedBytes, byte[] iv)
        {
            var dataSize = encryptedBytes.Length + iv.Length + 1;
            if (_cipherMode == CipherMode.GCM)
                dataSize += 1;

            var index = 0;
            var data = new byte[dataSize];
            data[index] = AesIvSize;
            index += 1;
            if (_cipherMode == CipherMode.GCM)
            {
                data[index] = GcmTagSize;
                index += 1;
            }

            Array.Copy(iv, 0, data, index, iv.Length);
            index += iv.Length;
            Array.Copy(encryptedBytes, 0, data, index, encryptedBytes.Length);

            return Convert.ToBase64String(data);
        }

        private (byte[], byte[], byte) UnpackCipherData(string cipherText)
        {
            var index = 0;
            var cipherData = Convert.FromBase64String(cipherText);
            byte ivSize = cipherData[index];
            index += 1;

            byte tagSize = 0;
            if (_cipherMode == CipherMode.GCM)
            {
                tagSize = cipherData[index];
                index += 1;
            }

            byte[] iv = new byte[ivSize];
            Array.Copy(cipherData, index, iv, 0, ivSize);
            index += ivSize;

            byte[] encryptedBytes = new byte[cipherData.Length - index];
            Array.Copy(cipherData, index, encryptedBytes, 0, encryptedBytes.Length);
            return (encryptedBytes, iv, tagSize);
        }
    }
}

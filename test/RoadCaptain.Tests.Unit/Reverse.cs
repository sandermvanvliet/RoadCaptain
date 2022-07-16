using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RoadCaptain.Tests.Unit
{
    public class Reverse
    {
        [Fact]
        public void Dwim()
        {
            var result = A(new byte[] { 62, 113, 116 });

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

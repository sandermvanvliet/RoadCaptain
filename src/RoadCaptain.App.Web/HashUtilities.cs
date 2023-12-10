﻿using System.Security.Cryptography;

namespace RoadCaptain.App.Web
{
    internal class HashUtilities
    {
        public static string HashAsHexString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input was empty", nameof(input));
            }
            
            var serializedBytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hashBytes = SHA256.HashData(serializedBytes);

            return Convert.ToHexString(hashBytes);
        }
    }
}
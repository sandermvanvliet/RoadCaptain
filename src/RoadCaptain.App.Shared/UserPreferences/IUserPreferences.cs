﻿using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace RoadCaptain.App.Shared.UserPreferences
{
    public abstract class UserPreferencesBase : IUserPreferences
    {
        private readonly JsonSerializerSettings _serializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = { new StringEnumConverter() }
        };

        protected UserPreferencesBase()
        {
            ConnectionSecret = GenerateSecret();
        }

        private static byte[] GenerateSecret()
        {
            // This is an AES 128 bit key
            var aes = Aes.Create();
            aes.KeySize = 128;
            aes.GenerateKey();
            return aes.Key;
        }

        public string? DefaultSport { get; set; }
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }
        public Point? InGameWindowLocation { get; set; }
        // This preference is transient and will not be stored
        public bool EndActivityAtEndOfRoute { get; set; }
        // This preference is transient and will not be stored
        public bool LoopRouteAtEndOfRoute { get; set; }
        public Version LastOpenedVersion { get; set; } = new Version(0, 0, 0, 0);
        public byte[]? ConnectionSecret { get; }
        public void Load()
        {
            var preferencesPath = GetPreferencesPath();

            if (!File.Exists(preferencesPath))
            {
                return;
            }

            var serializedContents = File.ReadAllText(preferencesPath, Encoding.UTF8);

            try
            {
                var storageObject = JsonConvert.DeserializeObject<UserPreferencesStorageObject>(serializedContents, _serializerSettings);

                DefaultSport = storageObject.DefaultSport;
                LastUsedFolder = storageObject.LastUsedFolder;
                Route = storageObject.Route;
                InGameWindowLocation = storageObject.InGameWindowLocation;
                LastOpenedVersion = storageObject.LastOpenedVersion ?? new Version(0, 0, 0, 0);
            }
            catch
            {
                // Nop
            }
        }

        public void Save()
        {
            var storageObject = new UserPreferencesStorageObject
            {
                DefaultSport = DefaultSport,
                InGameWindowLocation = InGameWindowLocation,
                LastUsedFolder = LastUsedFolder,
                Route = Route,
                LastOpenedVersion = GetType().Assembly.GetName().Version ?? new Version(0, 0, 0, 0)
            };

            var serializedContents = JsonConvert.SerializeObject(storageObject, Formatting.Indented, _serializerSettings);

            var preferencesPath = GetPreferencesPath();

            EnsureConfigDirectoryExists();

            File.WriteAllText(preferencesPath, serializedContents, Encoding.UTF8);
        }

        protected abstract void EnsureConfigDirectoryExists();

        protected abstract string GetPreferencesPath();
    }

    internal class UserPreferencesStorageObject
    {
        public string? DefaultSport { get; set; }
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }
        public Point? InGameWindowLocation { get; set; }
        public Version? LastOpenedVersion { get; set; }
    }
}
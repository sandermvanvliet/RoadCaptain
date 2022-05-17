using System;
using System.Drawing;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace RoadCaptain.App.Shared.UserPreferences
{
    public interface IUserPreferences
    {
        public string? DefaultSport { get; set; }
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }
        Point? InGameWindowLocation { get; set; }

        public void Load();
        public void Save();
    }

    public abstract class UserPreferencesBase : IUserPreferences
    {
        private readonly JsonSerializerSettings _serializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = { new StringEnumConverter() }
        };

        public string? DefaultSport { get; set; }
        public string? LastUsedFolder { get; set; }
        public string? Route { get; set; }
        public Point? InGameWindowLocation { get; set; }
        
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
                Route = Route
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
    }
}
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class WorldStoreToDisk : IWorldStore
    {
        private readonly string _worldsPath;
        private World[] _loadedWorlds;
        private readonly JsonSerializerSettings _serializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public WorldStoreToDisk() : this(Environment.CurrentDirectory)
        {
        }

        internal WorldStoreToDisk(string fileRoot)
        {
            _worldsPath = Path.Combine(fileRoot, "worlds.json");
        }

        public World[] LoadWorlds()
        {
            if (_loadedWorlds == null)
            {
                _loadedWorlds = JsonConvert.DeserializeObject<World[]>(
                    File.ReadAllText(_worldsPath), 
                    _serializerSettings);
            }

            return _loadedWorlds;
        }

        public World LoadWorldById(string id)
        {
            if (_loadedWorlds == null)
            {
                LoadWorlds();
            }

            return _loadedWorlds.SingleOrDefault(w => string.Equals(w.Id, id, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
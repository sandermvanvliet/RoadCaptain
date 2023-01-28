// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>
            {
                new SegmentDirectionConverter(),
                new StringEnumConverter()
            },
            NullValueHandling = NullValueHandling.Ignore
        };

        public WorldStoreToDisk() : this(Path.GetDirectoryName(typeof(WorldStoreToDisk).Assembly.Location))
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
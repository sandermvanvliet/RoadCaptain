using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class RouteStoreToDisk : IRouteStore
    {
        private static readonly JsonSerializerSettings RouteSerializationSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(new CamelCaseNamingStrategy())
            }
        };

        public PlannedRoute LoadFrom(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            return JsonConvert.DeserializeObject<PlannedRoute>(
                File.ReadAllText(path),
                RouteSerializationSettings);
        }

        public void Store(PlannedRoute route, string path)
        {
            var serialized = JsonConvert.SerializeObject(route, Formatting.Indented, RouteSerializationSettings);

            File.WriteAllText(path, serialized);
        }
    }
}

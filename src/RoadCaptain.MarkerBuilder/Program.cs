using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace RoadCaptain.MarkerBuilder
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var poiFiles = Directory.GetFiles(@"C:\git\temp\zwift\zwift-watopia-gpx\special_segments", "*.gpx");
            var world = "watopia";

            var markers = poiFiles
                .Select(file => Segment.FromGpx(File.ReadAllText(file)))
                .ToList();

            foreach (var segment in markers)
            {
                segment.Name = segment.Name.Replace($"({world})", "", StringComparison.InvariantCultureIgnoreCase).Trim();

                if (string.IsNullOrEmpty(segment.Id))
                {
                    segment.Id = segment.Name.Replace(" ", "-").ToLower();
                }
            }

            File.WriteAllText(
                $"markers-{world}.json",
                JsonConvert.SerializeObject(
                    markers,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Converters =
                        {
                            new StringEnumConverter()
                        }
                    }));
        }
    }
}
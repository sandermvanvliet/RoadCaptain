using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace RoadCaptain.SegmentSplitter
{
    public class Program
    {
        private readonly JsonSerializerSettings _serializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters =
            {
                new StringEnumConverter()
            }
        };

        public static void Main(string[] args)
        {
            var segmentFileName = args[0];
            var segmentToSplitId = args[1];

            new Program().Split(segmentToSplitId, segmentFileName);
        }

        public void Split(string segmentToSplitId, string segmentFileName)
        {
            var segments =
                JsonConvert.DeserializeObject<List<Segment>>(File.ReadAllText(segmentFileName), _serializerSettings);

            var segmentToSplit = segments.SingleOrDefault(s => s.Id == segmentToSplitId);

            if (segmentToSplit == null)
            {
                throw new Exception($"Segment '{segmentToSplitId}' not found");
            }

            string? splitPoint;

            while (true)
            {
                Console.WriteLine("Enter decimal coordinates of split point:");
                splitPoint = Console.ReadLine();

                if (!string.IsNullOrEmpty(splitPoint))
                {
                    if (splitPoint == "exit")
                    {
                        Console.WriteLine("Exiting...");
                        return;
                    }

                    break;
                }
            }

            var sliceIndex =
                segmentToSplit.Points.FindIndex(trackPoint => trackPoint.CoordinatesDecimal == splitPoint);

            if (sliceIndex == -1)
            {
                Console.WriteLine("Split point not found on segment, exiting...");
                return;
            }
            
            var beforeSplit = segmentToSplit.Slice("before", 0, sliceIndex);
            var afterSplit = segmentToSplit.Slice("after", sliceIndex);

            beforeSplit.CalculateDistances();
            afterSplit.CalculateDistances();

            segments.Remove(segmentToSplit);
            segments.Add(beforeSplit);
            segments.Add(afterSplit);

            File.WriteAllText(
                "split-segments.json",
                JsonConvert.SerializeObject(segments, Formatting.Indented, _serializerSettings));

            File.WriteAllText($"{beforeSplit.Id}.gpx", beforeSplit.AsGpx());
            File.WriteAllText($"{afterSplit.Id}.gpx", afterSplit.AsGpx());
        }
    }
}
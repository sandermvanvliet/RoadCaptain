using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace RoadCaptain.Tests.Benchmark
{
    [SimpleJob(RunStrategy.Throughput, warmupCount:1)]
    public class SegmentLoadingBenchmarkNewtonsoftJson
    {
        private readonly string _fileContentsOriginal;
        private readonly string _fileContentsOptimized;
        private readonly byte[] _binaryBytes;

        private readonly JsonSerializerSettings _serializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters =
            {
                new StringEnumConverter()
            }
        };

        public SegmentLoadingBenchmarkNewtonsoftJson()
        {
            _fileContentsOriginal = File.ReadAllText("original-segments-watopia.json");
            _fileContentsOptimized = File.ReadAllText("optimized-segments-watopia.json");
            _binaryBytes = File.ReadAllBytes("binary-segments-watopia.bin");
        }

        [Params(1000)]
        public int N;


        [Benchmark(Baseline = true)]
        public void DeserializeFromOriginalFile()
        {
            var segments = JsonConvert.DeserializeObject<List<Segment>>(_fileContentsOriginal, _serializerSettings);
        }

        [Benchmark]
        public void DeserializeFromOptimizedFile()
        {
            var segments = JsonConvert.DeserializeObject<List<Segment>>(_fileContentsOptimized, _serializerSettings);
        }

        [Benchmark]
        public void BinaryDeserialize()
        {
            using var reader = new BinaryReader(new MemoryStream(_binaryBytes));

            var readSegments = BinarySegmentSerializer.DeserializeSegment(reader);
        }
    }
}
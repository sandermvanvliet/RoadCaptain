// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Runtime.Serialization.Formatters.Binary;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace RoadCaptain.Tests.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<SegmentLoadingBenchmarkNewtonsoftJson>();
        }
    }
}

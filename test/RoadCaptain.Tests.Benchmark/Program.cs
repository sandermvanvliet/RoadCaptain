// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using BenchmarkDotNet.Running;

namespace RoadCaptain.Tests.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<SegmentLoadingBenchmark>();
        }
    }
}

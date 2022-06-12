using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace RoadCaptain.Tests.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<TrackPointBenchmarks>();
        }
    }

    public class TrackPointBenchmarks
    {
        [Benchmark]
        public void FromGameLocation()
        {

            for (int i = 0; i < 10000; i++)
                TrackPoint.FromGameLocation(12, 13, 14, ZwiftWorldId.Watopia);
        }
    }
}
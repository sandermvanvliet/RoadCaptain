using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
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
    
    [SimpleJob(RunStrategy.Throughput, warmupCount:1)]
    public class TrackPointBenchmarks
    {
        [Params(10000)]
        public int N;

        [Benchmark(Baseline = true)]
        public TrackPoint Baseline()
        {
            return TrackPoint.FromGameLocationBaseline(12, 13, 14, ZwiftWorldId.Watopia);
        }

        //[Benchmark(Baseline = false)]
        //public TrackPoint FastSwitch()
        //{
        //    return TrackPoint.FromGameLocation(12, 13, 14, ZwiftWorldId.Watopia);
        //}

        [Benchmark(Baseline = false)]
        public TrackPoint Unrolled()
        {
            return TrackPoint.FromGameLocationUnroll(12, 13, 14, ZwiftWorldId.Watopia);
        }

        //[Benchmark(Baseline = false)]
        //public TrackPoint UnrolledPreMul()
        //{
        //    return TrackPoint.FromGameLocationUnrollMul(12, 13, 14, ZwiftWorldId.Watopia);
        //}

        [Benchmark(Baseline = false)]
        public TrackPoint UnrolledNoDivision()
        {
            return TrackPoint.FromGameLocationUnrollNoDivision(12, 13, 14, ZwiftWorldId.Watopia);
        }

        [Benchmark(Baseline = false)]
        public TrackPoint Inlined()
        {
            return TrackPoint.FromGameLocationInlined(12, 13, 14, ZwiftWorldId.Watopia);
        }
    }
}
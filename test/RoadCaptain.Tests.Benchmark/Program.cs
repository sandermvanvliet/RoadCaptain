// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
            return FromGameLocationBaseline(12, 13, 14, ZwiftWorldId.Watopia);
        }

        [Benchmark(Baseline = false)]
        public TrackPoint Inlined()
        {
            return new GameCoordinate(12, 13, 14, ZwiftWorldId.Watopia).ToTrackPoint();
        }

        public static TrackPoint FromGameLocationBaseline(double latitudeOffsetCentimeters, double longitudeOffsetCentimeters,
            double altitude, ZwiftWorldId worldId)
        {
            ZwiftWorldConstants worldConstants;
            
            switch (worldId)
            {
                case ZwiftWorldId.Watopia:
                    worldConstants = TrackPoint.ZwiftWorlds[worldId];
                    break;
                case ZwiftWorldId.MakuriIslands:
                    worldConstants = TrackPoint.ZwiftWorlds[worldId];
                    break;
                default:
                    return TrackPoint.Unknown;
            }

            var latitudeAsCentimetersFromOrigin = latitudeOffsetCentimeters + worldConstants.CenterLatitudeFromOrigin;
            var latitude = latitudeAsCentimetersFromOrigin / worldConstants.MetersBetweenLatitudeDegree / 100;

            var longitudeAsCentimetersFromOrigin = longitudeOffsetCentimeters + worldConstants.CenterLongitudeFromOrigin;
            var longitude = longitudeAsCentimetersFromOrigin / worldConstants.MetersBetweenLongitudeDegree / 100;

            return new TrackPoint(latitude, longitude, altitude);
        }
    }
}

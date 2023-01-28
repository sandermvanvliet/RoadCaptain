// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace RoadCaptain.SegmentBuilder
{
    class Program
    {

        static void Main(string[] args)
        {
            var gpxDirectory = args.Length > 0 ? args[0] : @"C:\git\temp\zwift\ven-top";

            new Program().Run(gpxDirectory);
        }

        private readonly List<Segment> _segments = new();

        public static readonly JsonSerializerSettings SerializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters =
            {
                new StringEnumConverter()
            }
        };

        public void Run(string gpxDirectory)
        {
            Console.WriteLine("\n==== STEP 1 ====");
            GpxToSegmentsStep.Run(_segments, gpxDirectory);
            
            Console.WriteLine("\n==== STEP 2 ====");
            CleanupStep.Run(_segments);

            SegmentSmootherStep.Run(_segments);
            
            Console.WriteLine("\n==== STEP 3 ====");
            JunctionAlignmentStep.Run(_segments);
            
            Console.WriteLine("\n==== STEP 4 ====");
            JunctionSplitterStep.Run(_segments);
            
            Console.WriteLine("\n==== STEP 5 ====");
            TurnFinderStep.Run(_segments, gpxDirectory);
            
            Console.WriteLine("\n==== STEP 6 ====");
            CleanupStep.Run(_segments);
            
            Console.WriteLine("\n==== STEP 7 ====");
            OutputStep.Run(_segments, gpxDirectory);
            
            Console.WriteLine("\n==== STEP 8 ====");
            SpawnPointFinderStep.Run(_segments, gpxDirectory);
        }
    }
}


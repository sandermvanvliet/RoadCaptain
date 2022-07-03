// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
            var gpxDirectory = args.Length > 0 ? args[0] : @"C:\git\temp\zwift\zwift-makuri_islands-gpx";

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
            GpxToSegmentsStep.Run(_segments, gpxDirectory);

            CleanupStep.Run(_segments);

            JunctionAlignmentStep.Run(_segments);

            JunctionSplitterStep.Run(_segments);

            //TurnFinderStep.Run(_segments, gpxDirectory);

            //CleanupStep.Run(_segments);

            //SegmentSplitStep.Run(_segments);

            OutputStep.Run(_segments, gpxDirectory);
        }
    }
}


// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace RoadCaptain.SegmentBuilder
{
    class Program
    {

        static void Main(string[] args)
        {
            var gpxDirectory = args.Length > 0 ? args[0] : @"C:\git\temp\zwift\zwift-makuri-islands-gpx";

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

        private readonly Step[] _steps;
        private readonly ILogger _logger;

        public Program()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            _steps = new Step[]
            {
                new GpxToSegmentsStep(_logger),
                new RemoveSegmentsShorterThan20Meters(_logger),
                new SegmentSmootherStep(_logger),
                new JunctionAlignmentStep(_logger),
                new JunctionSplitterStep(_logger),
                new TurnFinderStep(_logger),
                new RemoveSegmentsShorterThan20Meters(_logger),
                new OutputStep(_logger),
                new SpawnPointFinderStep(_logger)
            };
        }
        
        public void Run(string gpxDirectory)
        {
            var context = new Context(new List<Segment>(), gpxDirectory);

            for (var step = 0; step < _steps.Length; step++)
            {
                _logger.Information("=== STEP {Step}: {StepName} ====", step, _steps[step].GetType().Name);
                
                var newContext = _steps[step].Run(context);

                context = newContext;
            }
        }
    }
}


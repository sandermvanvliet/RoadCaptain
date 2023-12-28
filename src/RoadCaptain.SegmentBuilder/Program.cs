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
            var world = "watopia";
            
            var gpxDirectory = args.Length > 0 ? args[0] : $@"C:\git\temp\zwift\zwift-{world}-gpx";

            new Program().Run(gpxDirectory, null, null, gpxDirectory, world);
        }
        
        public static readonly JsonSerializerSettings SerializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters =
            {
                new StringEnumConverter()
            },
            Formatting = Formatting.Indented
        };

        private readonly BaseStep[] _steps;
        private readonly ILogger _logger;

        public Program()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            _steps = new BaseStep[]
            {
                new GpxToSegmentsStep(0, _logger),
                new RemoveSegmentsShorterThan20Meters(1, _logger),
                new RemoveSegmentsWithNoElevationChange(2, _logger),
                new SegmentSmootherStep(3, _logger),
                new JunctionAlignmentStep(4, _logger),
                new JunctionSplitterStep(5, _logger),
                new TurnFinderStep(6, _logger),
                new RemoveSegmentsShorterThan20Meters(7, _logger),
                new RemoveSegmentsWithFewPoints(8, _logger),
                new OutputStep(9, _logger),
                new SpawnPointFinderStep(10, _logger)
            };
        }
        
        public void Run(string gpxDirectory, int? runFromStep, int? runUntilStepInclusive, string contextPath,
            string world)
        {
            runFromStep ??= 0;
            runUntilStepInclusive ??= _steps.Length - 1;

            Context context;

            if (runFromStep == 0)
            {
                context = new Context(0, new List<Segment>(), gpxDirectory, world);
            }
            else
            {
                context = Context.Load(runFromStep.Value, contextPath);
                // We start from this step but we don't want to re-run it,
                // so bump to the next step
                runFromStep = runFromStep.Value + 1;
            }

            for (var step = runFromStep.Value; step <= runUntilStepInclusive.Value; step++)
            {
                _logger.Information("=== STEP {Step}: {StepName} ====", step, _steps[step].GetType().Name);
                
                var newContext = _steps[step].Run(context);

                if (newContext.GetHashCode() == context.GetHashCode())
                {
                    throw new InvalidOperationException(
                        "The resulting context is exactly the same as the input context. Did you forget to create a new result context?");
                }

                context = newContext;

                context.Persist(contextPath);
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using RoadCaptain.Adapters;
using RoadCaptain.GameStates;
using Xunit;

namespace RoadCaptain.Tests.Unit.GameState.Repro
{
    public class RouteReproductionTests
    {
        [Fact]
        public void HillyKOMBypassRebelRoute_DoesNotSkipSegment()
        {
            var segments = new SegmentStore(@"c:\git\RoadCaptain\src\RoadCaptain.Adapters")
                .LoadSegments(
                    new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia },
                    SportType.Cycling);

            var hillyKomRoute = new SegmentSequenceBuilder()
                .StartingAt("watopia-bambino-fondo-001-after-after-after-after-after-after")
                .GoingStraightTo("watopia-bambino-fondo-001-after-before")
                .GoingStraightTo("watopia-bambino-fondo-002-before-before-before")
                .GoingStraightTo("watopia-bambino-fondo-002-before-before-after")
                .TurningRightTo("watopia-climbers-gambit-001")
                .TurningRightTo("watopia-bambino-fondo-002-before-before-after")
                .EndingAt("watopia-bambino-fondo-002-before-before-after")
                .Build();

            hillyKomRoute.EnteredSegment(hillyKomRoute.RouteSegmentSequence[0].SegmentId);
            hillyKomRoute.EnteredSegment(hillyKomRoute.RouteSegmentSequence[1].SegmentId);
            hillyKomRoute.EnteredSegment(hillyKomRoute.RouteSegmentSequence[2].SegmentId);

            var positions = new List<TrackPoint>();

            var segment = segments.Single(s => s.Id == hillyKomRoute.RouteSegmentSequence[2].SegmentId);
            var climbersGambitSegment = segments.Single(s => s.Id == "watopia-climbers-gambit-001");
            var nextSegment = segments.Single(s => s.Id == hillyKomRoute.RouteSegmentSequence[3].SegmentId);

            positions.AddRange(segment.Points.Skip(segment.Points.Count - 10).ToList());
            positions.AddRange(nextSegment.Points.Take(2));
            positions.Add(climbersGambitSegment.Points.Last());
            positions.AddRange(nextSegment.Points.Skip(2).Take(10));

            GameStates.GameState state = new OnRouteState(
                1,
                1,
                positions[0],
                segment,
                hillyKomRoute,
                SegmentDirection.AtoB,
                0,
                0,
                0);

            for (var index = 1; index < positions.Count; index++)
            {
                var newState = state.UpdatePosition(positions[index], segments, hillyKomRoute);

                // We only expect OnRouteState on this sequence of TrackPoints
                newState.Should().BeOfType<OnRouteState>();

                state = newState;
            }

            hillyKomRoute
                .SegmentSequenceIndex
                .Should()
                .Be(3);
        }

        [Fact]
        public void StartingVolcanoClimb()
        {
            var fileRoot = @"c:\git\RoadCaptain\src\RoadCaptain.Adapters";
            var segmentStore = new SegmentStore(fileRoot);
            var routeStore = new RouteStoreToDisk(segmentStore, new WorldStoreToDisk(fileRoot));
            var segments = segmentStore
                .LoadSegments(
                    new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia },
                    SportType.Cycling);

            var plannedRoute = routeStore.LoadFrom(@"C:\git\temp\zwift\RoadCaptain-troubleshoot\101-volcano-climb\VolcanoClimbRepro.json");

            plannedRoute.EnteredSegment(plannedRoute.RouteSegmentSequence[0].SegmentId);
            plannedRoute.EnteredSegment(plannedRoute.RouteSegmentSequence[1].SegmentId);

            var positions = File
                .ReadAllLines("GameState\\Repro\\VolcanoClimbJunction.json")
                .Select(JsonConvert.DeserializeObject<TrackPoint>)
                .ToList();

            var segment = segments.Single(s => s.Id == plannedRoute.RouteSegmentSequence[1].SegmentId);

            var startingPosition = segment
                .Points
                .Where(p => p.IsCloseTo(positions[0]))
                .OrderBy(p => p.DistanceTo(positions[0]))
                .First();

            GameStates.GameState state = new OnRouteState(
                1,
                1,
                startingPosition,
                segment,
                plannedRoute,
                SegmentDirection.BtoA,
                0,
                0,
                0);

            // Make turns available
            state = state.TurnCommandAvailable("turnleft");
            state = state.TurnCommandAvailable("gostraight");

            for (var index = 1; index < positions.Count; index++)
            {
                var newState = state.UpdatePosition(positions[index], segments, plannedRoute);

                // We only expect OnRouteState on this sequence of TrackPoints
                if (newState is not UpcomingTurnState and not OnRouteState)
                {
                    Debugger.Break();
                    throw new Exception($"Expected either {nameof(UpcomingTurnState)} or {nameof(OnRouteState)} but got {state.GetType().Name}");
                }

                state = newState;
            }
        }

        [Fact]
        public void RouteJsonToSegmentSequenceBuilder()
        {
            var fileRoot = @"c:\git\RoadCaptain\src\RoadCaptain.Adapters";
            var routeStore = new RouteStoreToDisk(new SegmentStore(fileRoot), new WorldStoreToDisk(fileRoot));

            var plannedRoute = routeStore.LoadFrom(@"C:\git\temp\zwift\RoadCaptain-troubleshoot\101-volcano-climb\VolcanoClimbRepro.json");

            var output = new StringBuilder();

            output.AppendLine("new SegmentSequenceBuilder()");
            output.AppendLine($"\t.StartingAt(\"{plannedRoute.StartingSegmentId}\")");

            foreach (var segmentSequence in plannedRoute.RouteSegmentSequence)
            {
                string methodName;
                string? nextSegment;

                switch (segmentSequence.TurnToNextSegment)
                {
                    case TurnDirection.GoStraight:
                        methodName = "GoingStraightTo";
                        nextSegment = segmentSequence.NextSegmentId;
                        break;
                    case TurnDirection.Left:
                        methodName = "TurningLeftTo";
                        nextSegment = segmentSequence.NextSegmentId;
                        break;
                    case TurnDirection.Right:
                        methodName = "TurningRightTo";
                        nextSegment = segmentSequence.NextSegmentId;
                        break;
                    default:
                        methodName = "EndingAt";
                        nextSegment = segmentSequence.SegmentId;
                        break;
                }
                
                output.AppendLine($"\t.{methodName}(\"{nextSegment}\")");
            }

            output.AppendLine("\t.Build();");

            Debug.WriteLine(output.ToString());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            var segments = new SegmentStore()
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

            foreach (var seq in hillyKomRoute.RouteSegmentSequence)
            {
                if (seq.NextSegmentId == null)
                {
                    continue;
                }

                var current = segments.Single(s => s.Id == seq.SegmentId);

                if (current.NextSegmentsNodeB.Any(n => n.SegmentId == seq.NextSegmentId))
                {
                    seq.Direction = SegmentDirection.AtoB;
                }
                else if (current.NextSegmentsNodeA.Any(n => n.SegmentId == seq.NextSegmentId))
                {
                    seq.Direction = SegmentDirection.BtoA;
                }
            }

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
                if (newState is not UpcomingTurnState and not OnRouteState)
                {
                    Debugger.Break();
                    throw new Exception($"Expected either {nameof(UpcomingTurnState)} or {nameof(OnRouteState)} but got {newState.GetType().Name}");
                }

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
            var segmentStore = new SegmentStore();
            var routeStore = new RouteStoreToDisk(segmentStore, new WorldStoreToDisk());
            var segments = segmentStore
                .LoadSegments(
                    new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia },
                    SportType.Cycling);

            var plannedRoute = routeStore.LoadFrom(@"GameState\Repro\VolcanoClimbRepro-2.json");

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
                    throw new Exception($"Expected either {nameof(UpcomingTurnState)} or {nameof(OnRouteState)} but got {newState.GetType().Name}");
                }

                state = newState;
            }
        }

        [Fact]
        public void VolcanoClimbLostRouteLockRepro()
        {
            // This test is to verify that for the Volcano Climb route, the route lock is
            // never lost. This used to happen because there was a non-routable segment
            // at the junction on the Volcano circuit and the land bridge towards the
            // Italian villas.
            // What would happen was that RoadCaptain flipped quickly between on/off route
            // states.
            var segmentStore = new SegmentStore();
            var routeStore = new RouteStoreToDisk(segmentStore, new WorldStoreToDisk());
            var segments = segmentStore
                .LoadSegments(
                    new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia },
                    SportType.Cycling);

            var plannedRoute = routeStore.LoadFrom(@"GameState\Repro\VolcanoClimbRepro-2.json");
            
            var positions = File
                .ReadAllLines("GameState\\Repro\\VolcanoClimbLostRouteLock-positions.json")
                .Select(JsonConvert.DeserializeObject<TrackPoint>)
                .ToList();
            
            GameStates.GameState state = new PositionedState(1, 2, positions[0]);
            var isOnRoute = false;

            for (var index = 1; index < positions.Count; index++)
            {
                var currentPosition = positions[index];

                var newState = state.UpdatePosition(currentPosition, segments, plannedRoute);

                // We start with PositionedState and OnSegmentState
                // before we enter the route and transition to OnRouteState
                if (newState is OnRouteState && !isOnRoute)
                {
                    isOnRoute = true;
                }

                if (isOnRoute)
                {
                    // Once on the route we always expect OnRouteState
                    newState
                        .Should()
                        .BeOfType<OnRouteState>();
                }

                // We don't ever want to see LostRouteLockState
                newState
                    .Should()
                    .NotBeOfType<LostRouteLockState>();

                state = newState;
            }
        }

        [Fact]
        public void VolcanoClimbLostRouteLockReproNumber2()
        {
            // This test is to verify that for the Volcano Climb route, the route lock is
            // never lost. This used to happen because there was a non-routable segment
            // at the junction on the Volcano circuit and the land bridge towards the
            // Italian villas.
            // What would happen was that RoadCaptain flipped quickly between on/off route
            // states.
            var segmentStore = new SegmentStore();
            var routeStore = new RouteStoreToDisk(segmentStore, new WorldStoreToDisk());
            var segments = segmentStore
                .LoadSegments(
                    new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia },
                    SportType.Cycling);

            var plannedRoute = routeStore.LoadFrom(@"GameState\Repro\VolcanoClimbRepro-2.json");
            
            var positions = File
                .ReadAllLines("GameState\\Repro\\VolcanoClimbUpcomingTurnToPositioned-positions.json")
                .Select(JsonConvert.DeserializeObject<TrackPoint>)
                .ToList();
            
            GameStates.GameState state = new PositionedState(1, 2, positions[0]);
            var isOnRoute = false;

            for (var index = 1; index < positions.Count; index++)
            {
                var currentPosition = positions[index];

                var newState = state.UpdatePosition(currentPosition, segments, plannedRoute);

                // We start with PositionedState and OnSegmentState
                // before we enter the route and transition to OnRouteState
                if (newState is OnRouteState && !isOnRoute)
                {
                    isOnRoute = true;
                }

                if (isOnRoute)
                {
                    // Once on the route we always expect OnRouteState
                    newState
                        .Should()
                        .BeOfType<OnRouteState>();
                }

                // We don't ever want to see LostRouteLockState
                newState
                    .Should()
                    .NotBeOfType<LostRouteLockState>();

                state = newState;
            }
        }

        [Fact]
        public void ItalianVillaSprintLoopLostRouteLock()
        {
            // This test is to verify that for the Volcano Climb route, the route lock is
            // never lost. This used to happen because there was a non-routable segment
            // at the junction on the Volcano circuit and the land bridge towards the
            // Italian villas.
            // What would happen was that RoadCaptain flipped quickly between on/off route
            // states.
            var segmentStore = new SegmentStore();
            var routeStore = new RouteStoreToDisk(segmentStore, new WorldStoreToDisk());
            var segments = segmentStore
                .LoadSegments(
                    new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia },
                    SportType.Cycling);

            var plannedRoute = routeStore.LoadFrom(@"GameState\Repro\Rebel.Route-Italian.Villa.Sprint.Loop.json");

            plannedRoute.EnteredSegment(plannedRoute.RouteSegmentSequence[0].SegmentId);
            plannedRoute.EnteredSegment(plannedRoute.RouteSegmentSequence[1].SegmentId);
            var secondSegment = segments.Single(s => s.Id == plannedRoute.RouteSegmentSequence[1].SegmentId);
            var wrongSegment = segments.Single(s => s.Id == plannedRoute.RouteSegmentSequence.Last().SegmentId);

            GameStates.GameState state = new OnRouteState(1, 2, secondSegment.Points[4], secondSegment, plannedRoute, SegmentDirection.AtoB, 0, 0, 0);

            var positions = new List<TrackPoint>();

            positions.AddRange(secondSegment.Points.Take(5).Reverse().ToList());
            positions.AddRange(wrongSegment.Points.AsEnumerable().Reverse().Take(20).ToList());

            for (var index = 1; index < positions.Count; index++)
            {
                var newState = state.UpdatePosition(positions[index], segments, plannedRoute);

                // We only expect OnRouteState on this sequence of TrackPoints
                if (newState is not LostRouteLockState)
                {
                    Debugger.Break();
                    throw new Exception($"Expected {nameof(LostRouteLockState)} but got {newState.GetType().Name}");
                }

                state = newState;
            }
        }

        [Fact]
        public void ItalianVillaSprintLoopLostRouteLockRecovery()
        {
            // This test is to verify that for the Volcano Climb route, the route lock is
            // never lost. This used to happen because there was a non-routable segment
            // at the junction on the Volcano circuit and the land bridge towards the
            // Italian villas.
            // What would happen was that RoadCaptain flipped quickly between on/off route
            // states.
            var segmentStore = new SegmentStore();
            var routeStore = new RouteStoreToDisk(segmentStore, new WorldStoreToDisk());
            var segments = segmentStore
                .LoadSegments(
                    new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia },
                    SportType.Cycling);

            var plannedRoute = routeStore.LoadFrom(@"GameState\Repro\Rebel.Route-Italian.Villa.Sprint.Loop.json");

            plannedRoute.EnteredSegment(plannedRoute.RouteSegmentSequence[0].SegmentId);
            plannedRoute.EnteredSegment(plannedRoute.RouteSegmentSequence[1].SegmentId);
            var secondSegment = segments.Single(s => s.Id == plannedRoute.RouteSegmentSequence[1].SegmentId);
            
            var positions = new List<TrackPoint>();

            positions.AddRange(secondSegment.Points.Take(25));
            positions.AddRange(secondSegment.Points.Skip(20).Take(5).Reverse().ToList());
            positions.AddRange(secondSegment.Points.Skip(20).Take(5).ToList());

            GameStates.GameState state = new OnRouteState(1, 2, positions[0], secondSegment, plannedRoute, SegmentDirection.AtoB, 0, 0, 0);

            for (var index = 1; index < positions.Count; index++)
            {
                var newState = state.UpdatePosition(positions[index], segments, plannedRoute);

                // We only expect OnRouteState on this sequence of TrackPoints
                if (index is > 25 and < 31)
                {
                    if (newState is not LostRouteLockState)
                    {
                        Debugger.Break();
                        throw new Exception($"Expected {nameof(LostRouteLockState)} but got {newState.GetType().Name}");
                    }
                }
                else
                {
                    if (newState is not OnRouteState)
                    {
                        Debugger.Break();
                        throw new Exception($"Expected {nameof(OnRouteState)} but got {newState.GetType().Name}");
                    }
                }


                state = newState;
            }
        }
    }
}

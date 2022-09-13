using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
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
    }
}

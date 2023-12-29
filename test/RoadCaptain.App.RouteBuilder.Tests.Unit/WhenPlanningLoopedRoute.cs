// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RoadCaptain.Adapters;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.Ports;
using Xunit;

namespace RoadCaptain.App.RouteBuilder.Tests.Unit
{
    public class WhenPlanningLoopedRoute
    {
        [Fact]
        public void GivenRouteWithoutSegmentsAndAddingNewSegment_IsPossibleLoopIsFalse()
        {
            var segment = SegmentById("watopia-bambino-fondo-004-before-before");
            
            _viewModel.StartOn(segment);

            var result = _viewModel.IsPossibleLoop();

            result.IsPossibleLoop.Should().BeFalse();
        }

        [Fact]
        public void GivenSelectedSegmentConnectsToFirstSegmentOfRoute_IsPossibleLoopIsTrue()
        {
            _viewModel.StartOn(SegmentById("watopia-bambino-fondo-004-before-before"));
            _viewModel.NextStep(TurnDirection.Right, "watopia-bambino-fondo-001-after-after-after-before", SegmentById("watopia-bambino-fondo-001-after-after-after-before"), SegmentDirection.BtoA, SegmentDirection.BtoA);
            _viewModel.NextStep(TurnDirection.GoStraight, "watopia-bambino-fondo-002-after", SegmentById("watopia-bambino-fondo-002-after"), SegmentDirection.BtoA, SegmentDirection.BtoA);
            _viewModel.NextStep(TurnDirection.Left, "watopia-beach-island-loop-004", SegmentById("watopia-beach-island-loop-004"), SegmentDirection.BtoA, SegmentDirection.BtoA);
            _viewModel.NextStep(TurnDirection.Left, "watopia-bambino-fondo-001-after-after-after-after-before-before", SegmentById("watopia-bambino-fondo-001-after-after-after-after-before-before"), SegmentDirection.BtoA, SegmentDirection.BtoA);

            var result = _viewModel.IsPossibleLoop();

            result.IsPossibleLoop.Should().BeTrue();
        }

        [Fact]
        public void GivenSelectedSegmentConnectsToPreviousSegment_IsPossibleLoopIsFalse()
        {
            _viewModel.StartOn(SegmentById("watopia-bambino-fondo-004-before-after"));
            _viewModel.NextStep(TurnDirection.Right, "watopia-beach-island-loop-002", SegmentById("watopia-beach-island-loop-002"), SegmentDirection.BtoA, SegmentDirection.BtoA);
            _viewModel.NextStep(TurnDirection.GoStraight, "watopia-bambino-fondo-004-after-after", SegmentById("watopia-bambino-fondo-004-after-after"), SegmentDirection.BtoA, SegmentDirection.BtoA);
            _viewModel.NextStep(TurnDirection.GoStraight, "watopia-big-foot-hills-002", SegmentById("watopia-big-foot-hills-002"), SegmentDirection.AtoB, SegmentDirection.AtoB);

            var result = _viewModel.IsPossibleLoop();

            result.IsPossibleLoop.Should().BeFalse();
        }

        private readonly List<Segment> _segments;
        private readonly RouteViewModel _viewModel;

        public WhenPlanningLoopedRoute()
        {
            ISegmentStore segmentStore = new SegmentStore(new NopMonitoringEvents());
            var worldStore = new WorldStoreToDisk();
            var world = worldStore.LoadWorldById("watopia")!;

            IRouteStore routeStore = new RouteStoreToDisk(
                segmentStore, 
                worldStore);

            _segments = segmentStore.LoadSegments(world, SportType.Cycling);

            _viewModel = new RouteViewModel(routeStore, segmentStore);
            _viewModel.World = world;
            _viewModel.Sport = SportType.Cycling;
        }

        private Segment SegmentById(string id)
        {
            return _segments.Single(s => s.Id == id);
        }
    }
}


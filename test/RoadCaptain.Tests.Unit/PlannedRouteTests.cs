// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using FluentAssertions;
using Xunit;

namespace RoadCaptain.Tests.Unit
{
    public class PlannedRouteTests
    {
        private readonly PlannedRoute _plannedRoute;
        
        [Fact]
        public void SegmentZero()
        {
            _plannedRoute.EnteredSegment("seg-0");
            
            _plannedRoute.SegmentSequenceIndex.Should().Be(0);
            _plannedRoute.CurrentSegmentId.Should().Be("seg-0");
            _plannedRoute.NextSegmentId.Should().Be("seg-1");
            _plannedRoute.OnLeadIn.Should().BeTrue();
        }
        
        [Fact]
        public void SegmentOne()
        {
            _plannedRoute.EnteredSegment("seg-0");
            _plannedRoute.EnteredSegment("seg-1");
            
            _plannedRoute.SegmentSequenceIndex.Should().Be(1);
            _plannedRoute.CurrentSegmentId.Should().Be("seg-1");
            _plannedRoute.NextSegmentId.Should().Be("seg-2");
            _plannedRoute.OnLeadIn.Should().BeTrue();
        }

        [Fact]
        public void SegmentTwo()
        {
            _plannedRoute.EnteredSegment("seg-0");
            _plannedRoute.EnteredSegment("seg-1");
            _plannedRoute.EnteredSegment("seg-2");
            
            _plannedRoute.SegmentSequenceIndex.Should().Be(2);
            _plannedRoute.CurrentSegmentId.Should().Be("seg-2");
            _plannedRoute.NextSegmentId.Should().Be("seg-3");
            _plannedRoute.OnLeadIn.Should().BeFalse();
        }

        [Fact]
        public void SegmentThree()
        {
            _plannedRoute.EnteredSegment("seg-0");
            _plannedRoute.EnteredSegment("seg-1");
            _plannedRoute.EnteredSegment("seg-2");
            _plannedRoute.EnteredSegment("seg-3");
            
            _plannedRoute.SegmentSequenceIndex.Should().Be(3);
            _plannedRoute.CurrentSegmentId.Should().Be("seg-3");
            _plannedRoute.NextSegmentId.Should().Be("seg-4");
            _plannedRoute.OnLeadIn.Should().BeFalse();
        }

        [Fact]
        public void SegmentFour()
        {
            _plannedRoute.EnteredSegment("seg-0");
            _plannedRoute.EnteredSegment("seg-1");
            _plannedRoute.EnteredSegment("seg-2");
            _plannedRoute.EnteredSegment("seg-3");
            _plannedRoute.EnteredSegment("seg-4");
            
            _plannedRoute.SegmentSequenceIndex.Should().Be(4);
            _plannedRoute.CurrentSegmentId.Should().Be("seg-4");
            _plannedRoute.NextSegmentId.Should().Be("seg-2");
            _plannedRoute.OnLeadIn.Should().BeFalse();
        }

        [Fact]
        public void GivenLoopedRouteWithLeadInAndOnLastSegmentOfRoute_NextSegmentSequenceIsLoopStartSegmentSequence()
        {
            _plannedRoute.EnteredSegment("seg-0");
            _plannedRoute.EnteredSegment("seg-1");
            _plannedRoute.EnteredSegment("seg-2");
            _plannedRoute.EnteredSegment("seg-3");
            _plannedRoute.EnteredSegment("seg-4");

            _plannedRoute.OnLeadIn.Should().BeFalse();

            _plannedRoute
                .NextSegmentSequence!
                .SegmentId
                .Should()
                .Be("seg-2");
        }

        [Fact]
        public void GivenNextSegmentOnSecondLoop_SegmentSequenceIndexIsTwo()
        {
            _plannedRoute.EnteredSegment("seg-0");
            _plannedRoute.EnteredSegment("seg-1");
            _plannedRoute.EnteredSegment("seg-2");
            _plannedRoute.EnteredSegment("seg-3");
            _plannedRoute.EnteredSegment("seg-4");
            _plannedRoute.EnteredSegment(_plannedRoute.NextSegmentId!);
            
            _plannedRoute.SegmentSequenceIndex.Should().Be(2);

            _plannedRoute
                .CurrentSegmentSequence!
                .SegmentId
                .Should()
                .Be("seg-2");

            _plannedRoute
                .NextSegmentSequence!
                .SegmentId
                .Should()
                .Be("seg-3");

            _plannedRoute.OnLeadIn.Should().BeFalse();
        }

        public PlannedRouteTests()
        {
            _plannedRoute = new PlannedRoute
            {
                Sport = SportType.Cycling,
                World = new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia },
                RouteSegmentSequence =
                {
                    new SegmentSequence(direction: SegmentDirection.AtoB, nextSegmentId: "seg-1", segmentId: "seg-0",
                        type: SegmentSequenceType.LeadIn, turnToNextSegment: TurnDirection.GoStraight),
                    new SegmentSequence(direction: SegmentDirection.AtoB, nextSegmentId: "seg-2", segmentId: "seg-1",
                        type: SegmentSequenceType.LeadIn, turnToNextSegment: TurnDirection.GoStraight),
                    new SegmentSequence(direction: SegmentDirection.AtoB, nextSegmentId: "seg-3", segmentId: "seg-2",
                        type: SegmentSequenceType.LoopStart, turnToNextSegment: TurnDirection.GoStraight),
                    new SegmentSequence(direction: SegmentDirection.AtoB, nextSegmentId: "seg-4", segmentId: "seg-3",
                        type: SegmentSequenceType.Loop, turnToNextSegment: TurnDirection.GoStraight),
                    new SegmentSequence(direction: SegmentDirection.AtoB, nextSegmentId: "seg-2", segmentId: "seg-4",
                        type: SegmentSequenceType.LoopEnd, turnToNextSegment: TurnDirection.GoStraight),
                }
            };
        }
    }
}

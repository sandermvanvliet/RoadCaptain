// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using FluentAssertions;
using Xunit;

namespace RoadCaptain.Tests.Unit
{
    public class PlannedRouteTests
    {
        private readonly PlannedRoute _plannedRoute = new()
        {
            Sport = SportType.Cycling,
            World = new World { Id = "watopia", ZwiftId = ZwiftWorldId.Watopia },
            RouteSegmentSequence =
            {
                new SegmentSequence(direction: SegmentDirection.AtoB, segmentId: "seg-0", nextSegmentId: "seg-1", type: SegmentSequenceType.LeadIn, turnToNextSegment: TurnDirection.GoStraight),
                new SegmentSequence(direction: SegmentDirection.AtoB, segmentId: "seg-1", nextSegmentId: "seg-2", type: SegmentSequenceType.LeadIn, turnToNextSegment: TurnDirection.GoStraight),
                new SegmentSequence(direction: SegmentDirection.AtoB, segmentId: "seg-2", nextSegmentId: "seg-3", type: SegmentSequenceType.LoopStart, turnToNextSegment: TurnDirection.GoStraight),
                new SegmentSequence(direction: SegmentDirection.AtoB, segmentId: "seg-3", nextSegmentId: "seg-4", type: SegmentSequenceType.Loop, turnToNextSegment: TurnDirection.GoStraight),
                new SegmentSequence(direction: SegmentDirection.AtoB, segmentId: "seg-4", nextSegmentId: "seg-2", type: SegmentSequenceType.LoopEnd, turnToNextSegment: TurnDirection.GoStraight),
            },
            NumberOfLoops = 5
        };
        
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

        [Fact]
        public void GivenNextSegmentOnSecondLoop_CurrentSegmentIsSegment2()
        {
            _plannedRoute.EnteredSegment("seg-0");
            _plannedRoute.EnteredSegment("seg-1");
            _plannedRoute.EnteredSegment("seg-2");
            _plannedRoute.EnteredSegment("seg-3");
            _plannedRoute.EnteredSegment("seg-4");
            _plannedRoute.EnteredSegment(_plannedRoute.NextSegmentId!);

            _plannedRoute
                .CurrentSegmentSequence!
                .SegmentId
                .Should()
                .Be("seg-2");
        }

        [Fact]
        public void GivenNextSegmentOnSecondLoop_NextSegmentIsSegment3()
        {
            _plannedRoute.EnteredSegment("seg-0");
            _plannedRoute.EnteredSegment("seg-1");
            _plannedRoute.EnteredSegment("seg-2");
            _plannedRoute.EnteredSegment("seg-3");
            _plannedRoute.EnteredSegment("seg-4");
            _plannedRoute.EnteredSegment(_plannedRoute.NextSegmentId!);

            _plannedRoute
                .NextSegmentSequence!
                .SegmentId
                .Should()
                .Be("seg-3");
        }

        [Fact]
        public void GivenNextSegmentOnSecondLoop_RouteIsNotOnLeadIn()
        {
            _plannedRoute.EnteredSegment("seg-0");
            _plannedRoute.EnteredSegment("seg-1");
            _plannedRoute.EnteredSegment("seg-2");
            _plannedRoute.EnteredSegment("seg-3");
            _plannedRoute.EnteredSegment("seg-4");
            _plannedRoute.EnteredSegment(_plannedRoute.NextSegmentId!);

            _plannedRoute.OnLeadIn.Should().BeFalse();
        }

        [Fact]
        public void GivenLoopModeIsInfiniteAndOnLastSegment_NextSegmentIsLoopStart()
        {
            _plannedRoute.NumberOfLoops = 2;
            _plannedRoute.LoopMode = LoopMode.Infinite;

            _plannedRoute.EnteredSegment("seg-0");
            _plannedRoute.EnteredSegment("seg-1");

            // Loop 1
            _plannedRoute.EnteredSegment("seg-2");
            _plannedRoute.EnteredSegment("seg-3");
            _plannedRoute.EnteredSegment("seg-4");

            // Loop 2
            _plannedRoute.EnteredSegment("seg-2");
            _plannedRoute.EnteredSegment("seg-3");
            _plannedRoute.EnteredSegment("seg-4");

            _plannedRoute.NextSegmentId!.Should().Be("seg-2");
        }
    }
}

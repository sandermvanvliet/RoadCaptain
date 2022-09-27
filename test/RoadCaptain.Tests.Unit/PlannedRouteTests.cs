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
                .NextSegmentSequence
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
            _plannedRoute.EnteredSegment(_plannedRoute.NextSegmentId);
            
            _plannedRoute.SegmentSequenceIndex.Should().Be(2);

            _plannedRoute
                .CurrentSegmentSequence
                .SegmentId
                .Should()
                .Be("seg-2");

            _plannedRoute
                .NextSegmentSequence
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
                    new SegmentSequence { Direction = SegmentDirection.AtoB, NextSegmentId = "seg-1", SegmentId = "seg-0", Type = SegmentSequenceType.LeadIn, TurnToNextSegment = TurnDirection.GoStraight },
                    new SegmentSequence { Direction = SegmentDirection.AtoB, NextSegmentId = "seg-2", SegmentId = "seg-1", Type = SegmentSequenceType.LeadIn, TurnToNextSegment = TurnDirection.GoStraight },
                    new SegmentSequence { Direction = SegmentDirection.AtoB, NextSegmentId = "seg-3", SegmentId = "seg-2", Type = SegmentSequenceType.LoopStart, TurnToNextSegment = TurnDirection.GoStraight },
                    new SegmentSequence { Direction = SegmentDirection.AtoB, NextSegmentId = "seg-4", SegmentId = "seg-3", Type = SegmentSequenceType.Loop, TurnToNextSegment = TurnDirection.GoStraight },
                    new SegmentSequence { Direction = SegmentDirection.AtoB, NextSegmentId = "seg-2", SegmentId = "seg-4", Type = SegmentSequenceType.LoopEnd, TurnToNextSegment = TurnDirection.GoStraight },
                }
            };
        }
    }
}
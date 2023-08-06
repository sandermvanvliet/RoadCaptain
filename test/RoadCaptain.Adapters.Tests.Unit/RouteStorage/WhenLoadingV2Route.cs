using FluentAssertions;
using Newtonsoft.Json;

namespace RoadCaptain.Adapters.Tests.Unit.RouteStorage
{
    public class WhenLoadingV2Route
    {
        [Fact]
        public void GivenRouteIsLoopCreatedInVersion0640_SegmentTypesAreSet()
        {
            var plannedRoute = new PlannedRoute
            {
                World = new World { Id = "watopia" },
                Sport = SportType.Cycling,
                Name = "Test route that is a loop",
                WorldId = "watopia",
                ZwiftRouteName = "Test route"
            };

            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-0", "seg-1", SegmentDirection.AtoB, TurnDirection.GoStraight));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-1", "seg-2", SegmentDirection.AtoB, TurnDirection.GoStraight));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-2", "seg-3", SegmentDirection.AtoB, TurnDirection.GoStraight));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-3", "seg-0", SegmentDirection.AtoB, TurnDirection.None));

            var persistedRoute = new PersistedRouteVersion2
            {
                RoadCaptainVersion = "0.6.4.0",
                Route = plannedRoute
            };

            var serialized = JsonConvert.SerializeObject(persistedRoute, RouteStoreToDisk.RouteSerializationSettings);

            var deserializedAndUpgradedRoute = new RouteStoreToDisk(new StubSegmentStore(), new WorldStoreToDisk()).DeserializeAndUpgrade(serialized);

            deserializedAndUpgradedRoute.RouteSegmentSequence[0].Type.Should().Be(SegmentSequenceType.LoopStart);
            deserializedAndUpgradedRoute.RouteSegmentSequence[1].Type.Should().Be(SegmentSequenceType.Loop);
            deserializedAndUpgradedRoute.RouteSegmentSequence[2].Type.Should().Be(SegmentSequenceType.Loop);
            deserializedAndUpgradedRoute.RouteSegmentSequence[3].Type.Should().Be(SegmentSequenceType.LoopEnd);
        }
        
        [Fact]
        public void GivenRouteIsLoopCreatedInVersion0640_LoopModeIsSetToInfinite()
        {
            var plannedRoute = new PlannedRoute
            {
                World = new World { Id = "watopia" },
                Sport = SportType.Cycling,
                Name = "Test route that is a loop",
                WorldId = "watopia",
                ZwiftRouteName = "Test route"
            };

            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-0", "seg-1", SegmentDirection.AtoB, TurnDirection.GoStraight));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-1", "seg-2", SegmentDirection.AtoB, TurnDirection.GoStraight));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-2", "seg-3", SegmentDirection.AtoB, TurnDirection.GoStraight));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-3", "seg-0", SegmentDirection.AtoB, TurnDirection.None));

            var persistedRoute = new PersistedRouteVersion2
            {
                RoadCaptainVersion = "0.6.4.0",
                Route = plannedRoute
            };

            var serialized = JsonConvert.SerializeObject(persistedRoute, RouteStoreToDisk.RouteSerializationSettings);

            var deserializedAndUpgradedRoute = new RouteStoreToDisk(new StubSegmentStore(), new WorldStoreToDisk()).DeserializeAndUpgrade(serialized);
            
            deserializedAndUpgradedRoute.LoopMode.Should().Be(LoopMode.Infinite);
            deserializedAndUpgradedRoute.NumberOfLoops.Should().BeNull();
        }

        [Fact]
        public void GivenRouteIsLoopCreatedInVersion0650_SegmentTypesAreSet()
        {
            var plannedRoute = new PlannedRoute
            {
                World = new World { Id = "watopia" },
                Sport = SportType.Cycling,
                Name = "Test route that is a loop",
                WorldId = "watopia",
                ZwiftRouteName = "Test route"
            };

            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-0", "seg-1", SegmentDirection.AtoB, TurnDirection.GoStraight));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-1", "seg-2", SegmentDirection.AtoB, TurnDirection.GoStraight));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-2", "seg-3", SegmentDirection.AtoB, TurnDirection.GoStraight));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-3", "seg-0", SegmentDirection.AtoB, TurnDirection.None));

            var persistedRoute = new PersistedRouteVersion2
            {
                RoadCaptainVersion = "0.6.5.0",
                Route = plannedRoute
            };

            var serialized = JsonConvert.SerializeObject(persistedRoute, RouteStoreToDisk.RouteSerializationSettings);

            var deserializedAndUpgradedRoute = new RouteStoreToDisk(new StubSegmentStore(), new WorldStoreToDisk()).DeserializeAndUpgrade(serialized);

            deserializedAndUpgradedRoute.RouteSegmentSequence[0].Type.Should().Be(SegmentSequenceType.LoopStart);
            deserializedAndUpgradedRoute.RouteSegmentSequence[1].Type.Should().Be(SegmentSequenceType.Loop);
            deserializedAndUpgradedRoute.RouteSegmentSequence[2].Type.Should().Be(SegmentSequenceType.Loop);
            deserializedAndUpgradedRoute.RouteSegmentSequence[3].Type.Should().Be(SegmentSequenceType.LoopEnd);
        }

        [Fact]
        public void GivenRouteIsLoopCreatedInVersion0650_LoopModeIsSetToInfinite()
        {
            var plannedRoute = new PlannedRoute
            {
                World = new World { Id = "watopia" },
                Sport = SportType.Cycling,
                Name = "Test route that is a loop",
                WorldId = "watopia",
                ZwiftRouteName = "Test route"
            };

            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-0", "seg-1", SegmentDirection.AtoB, TurnDirection.GoStraight));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-1", "seg-2", SegmentDirection.AtoB, TurnDirection.GoStraight));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-2", "seg-3", SegmentDirection.AtoB, TurnDirection.GoStraight));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-3", "seg-0", SegmentDirection.AtoB, TurnDirection.None));

            var persistedRoute = new PersistedRouteVersion2
            {
                RoadCaptainVersion = "0.6.5.0",
                Route = plannedRoute
            };

            var serialized = JsonConvert.SerializeObject(persistedRoute, RouteStoreToDisk.RouteSerializationSettings);

            var deserializedAndUpgradedRoute = new RouteStoreToDisk(new StubSegmentStore(), new WorldStoreToDisk()).DeserializeAndUpgrade(serialized);

            deserializedAndUpgradedRoute.LoopMode.Should().Be(LoopMode.Infinite);
            deserializedAndUpgradedRoute.NumberOfLoops.Should().BeNull();
        }

        [Fact]
        public void GivenRouteIsLoopCreatedInVersion0660_LoopModeIsSetToInfinite()
        {
            var plannedRoute = new PlannedRoute
            {
                World = new World { Id = "watopia" },
                Sport = SportType.Cycling,
                Name = "Test route that is a loop",
                WorldId = "watopia",
                ZwiftRouteName = "Test route"
            };

            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-0", "seg-1", SegmentDirection.AtoB, TurnDirection.GoStraight, SegmentSequenceType.LoopStart));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-1", "seg-2", SegmentDirection.AtoB, TurnDirection.GoStraight, SegmentSequenceType.Loop));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-2", "seg-3", SegmentDirection.AtoB, TurnDirection.GoStraight, SegmentSequenceType.Loop));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-3", "seg-0", SegmentDirection.AtoB, TurnDirection.None, SegmentSequenceType.LoopEnd));

            var persistedRoute = new PersistedRouteVersion2
            {
                RoadCaptainVersion = "0.6.6.0",
                Route = plannedRoute
            };

            var serialized = JsonConvert.SerializeObject(persistedRoute, RouteStoreToDisk.RouteSerializationSettings);

            var deserializedAndUpgradedRoute = new RouteStoreToDisk(new StubSegmentStore(), new WorldStoreToDisk()).DeserializeAndUpgrade(serialized);

            deserializedAndUpgradedRoute.LoopMode.Should().Be(LoopMode.Infinite);
            deserializedAndUpgradedRoute.NumberOfLoops.Should().BeNull();
        }

        [Fact]
        public void GivenRouteIsLoopCreatedInVersion0680_LoopModeIsSetToInfinite()
        {
            var plannedRoute = new PlannedRoute
            {
                World = new World { Id = "watopia" },
                Sport = SportType.Cycling,
                Name = "Test route that is a loop",
                WorldId = "watopia",
                ZwiftRouteName = "Test route"
            };

            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-0", "seg-1", SegmentDirection.AtoB, TurnDirection.GoStraight, SegmentSequenceType.LoopStart));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-1", "seg-2", SegmentDirection.AtoB, TurnDirection.GoStraight, SegmentSequenceType.Loop));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-2", "seg-3", SegmentDirection.AtoB, TurnDirection.GoStraight, SegmentSequenceType.Loop));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-3", "seg-0", SegmentDirection.AtoB, TurnDirection.None, SegmentSequenceType.LoopEnd));

            var persistedRoute = new PersistedRouteVersion3
            {
                RoadCaptainVersion = "0.6.8.0",
                Route = plannedRoute
            };

            var serialized = JsonConvert.SerializeObject(persistedRoute, RouteStoreToDisk.RouteSerializationSettings);

            var deserializedAndUpgradedRoute = new RouteStoreToDisk(new StubSegmentStore(), new WorldStoreToDisk()).DeserializeAndUpgrade(serialized);

            deserializedAndUpgradedRoute.LoopMode.Should().Be(LoopMode.Infinite);
            deserializedAndUpgradedRoute.NumberOfLoops.Should().BeNull();
        }

        [Fact]
        public void GivenRouteIsLoopCreatedInVersion0690_LoopModeIsSetToInfinite()
        {
            var plannedRoute = new PlannedRoute
            {
                World = new World { Id = "watopia" },
                Sport = SportType.Cycling,
                Name = "Test route that is a loop",
                WorldId = "watopia",
                ZwiftRouteName = "Test route"
            };

            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-0", "seg-1", SegmentDirection.AtoB, TurnDirection.GoStraight, SegmentSequenceType.LoopStart));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-1", "seg-2", SegmentDirection.AtoB, TurnDirection.GoStraight, SegmentSequenceType.Loop));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-2", "seg-3", SegmentDirection.AtoB, TurnDirection.GoStraight, SegmentSequenceType.Loop));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-3", "seg-0", SegmentDirection.AtoB, TurnDirection.None, SegmentSequenceType.LoopEnd));

            var persistedRoute = new PersistedRouteVersion3
            {
                RoadCaptainVersion = "0.6.9.0",
                Route = plannedRoute
            };

            var serialized = JsonConvert.SerializeObject(persistedRoute, RouteStoreToDisk.RouteSerializationSettings);

            var deserializedAndUpgradedRoute = new RouteStoreToDisk(new StubSegmentStore(), new WorldStoreToDisk()).DeserializeAndUpgrade(serialized);

            deserializedAndUpgradedRoute.LoopMode.Should().Be(LoopMode.Infinite);
            deserializedAndUpgradedRoute.NumberOfLoops.Should().BeNull();
        }

        [Fact]
        public void GivenRouteIsLoopCreatedInVersion06101_LoopModeIsSetToInfinite()
        {
            var plannedRoute = new PlannedRoute
            {
                World = new World { Id = "watopia" },
                Sport = SportType.Cycling,
                Name = "Test route that is a loop",
                WorldId = "watopia",
                ZwiftRouteName = "Test route"
            };

            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-0", "seg-1", SegmentDirection.AtoB, TurnDirection.GoStraight, SegmentSequenceType.LoopStart));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-1", "seg-2", SegmentDirection.AtoB, TurnDirection.GoStraight, SegmentSequenceType.Loop));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-2", "seg-3", SegmentDirection.AtoB, TurnDirection.GoStraight, SegmentSequenceType.Loop));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-3", "seg-0", SegmentDirection.AtoB, TurnDirection.None, SegmentSequenceType.LoopEnd));

            var persistedRoute = new PersistedRouteVersion3
            {
                RoadCaptainVersion = "0.6.10.1",
                Route = plannedRoute
            };

            var serialized = JsonConvert.SerializeObject(persistedRoute, RouteStoreToDisk.RouteSerializationSettings);

            var deserializedAndUpgradedRoute = new RouteStoreToDisk(new StubSegmentStore(), new WorldStoreToDisk()).DeserializeAndUpgrade(serialized);

            deserializedAndUpgradedRoute.LoopMode.Should().Be(LoopMode.Infinite);
            deserializedAndUpgradedRoute.NumberOfLoops.Should().BeNull();
        }

        [Fact]
        public void GivenRouteIsLoopCreatedInVersion0700_LoopModeIsTakenFromSerializedRoute()
        {
            var plannedRoute = new PlannedRoute
            {
                World = new World { Id = "watopia" },
                Sport = SportType.Cycling,
                Name = "Test route that is a loop",
                WorldId = "watopia",
                ZwiftRouteName = "Test route",
                LoopMode = LoopMode.Constrained,
                NumberOfLoops = 5
            };

            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-0", "seg-1", SegmentDirection.AtoB, TurnDirection.GoStraight, SegmentSequenceType.LoopStart));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-1", "seg-2", SegmentDirection.AtoB, TurnDirection.GoStraight, SegmentSequenceType.Loop));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-2", "seg-3", SegmentDirection.AtoB, TurnDirection.GoStraight, SegmentSequenceType.Loop));
            plannedRoute.RouteSegmentSequence.Add(new SegmentSequence("seg-3", "seg-0", SegmentDirection.AtoB, TurnDirection.None, SegmentSequenceType.LoopEnd));

            var persistedRoute = new PersistedRouteVersion3
            {
                RoadCaptainVersion = "0.7.0.0",
                Route = plannedRoute
            };

            var serialized = JsonConvert.SerializeObject(persistedRoute, RouteStoreToDisk.RouteSerializationSettings);

            var deserializedAndUpgradedRoute = new RouteStoreToDisk(new StubSegmentStore(), new WorldStoreToDisk()).DeserializeAndUpgrade(serialized);

            deserializedAndUpgradedRoute.LoopMode.Should().Be(LoopMode.Constrained);
            deserializedAndUpgradedRoute.NumberOfLoops.Should().Be(5);
        }
    }
}

// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("RoadCaptain.Tests.Unit")]
[assembly:InternalsVisibleTo("RoadCaptain.Tests.Benchmark")]
[assembly:InternalsVisibleTo("RoadCaptain.Adapters.Tests.Unit")]
[assembly:InternalsVisibleTo("RoadCaptain.App.Runner.Tests.Unit")]
[assembly:InternalsVisibleTo("RoadCaptain.App.RouteBuilder.Tests.Unit")]
[assembly:InternalsVisibleTo("RoadCaptain.SegmentSplitter")]
[assembly:InternalsVisibleTo("RoadCaptain.SegmentJoiner")]
[assembly:InternalsVisibleTo("RoadCaptain.SegmentBuilder")]
[assembly:InternalsVisibleTo("RoadCaptain.SegmentSerializer")]

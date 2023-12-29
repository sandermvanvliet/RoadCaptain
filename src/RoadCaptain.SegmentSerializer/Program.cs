// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using RoadCaptain.Adapters;

var segmentStore = new SegmentStore(@"c:\git\RoadCaptain\src\RoadCaptain.Adapters", null!);
segmentStore.SerializeToBinary();

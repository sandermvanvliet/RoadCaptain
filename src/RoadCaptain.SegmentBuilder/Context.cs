// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Newtonsoft.Json;

namespace RoadCaptain.SegmentBuilder
{
    internal class Context
    {
        public Context(int step, List<Segment> segments, string gpxDirectory, string world)
        {
            Segments = segments.ToImmutableList();
            Step = step;
            GpxDirectory = gpxDirectory;
            World = world;
        }

        public ImmutableList<Segment> Segments { get; }
        public int Step { get; }
        public string GpxDirectory { get; }
        public string World { get; }

        public void Persist(string contextPath)
        {
            File.WriteAllText(Path.Combine(contextPath, $"context-{Step}.json"), JsonConvert.SerializeObject(this, Program.SerializerSettings));
        }

        public static Context Load(int step, string contextPath)
        {
            var serialized = File.ReadAllText(Path.Combine(contextPath, $"context-{step}.json"));

            return JsonConvert.DeserializeObject<Context>(serialized, Program.SerializerSettings)!;
        }
    }
}

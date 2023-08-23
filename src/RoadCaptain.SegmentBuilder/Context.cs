using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Newtonsoft.Json;

namespace RoadCaptain.SegmentBuilder
{
    internal class Context
    {
        public Context(int step, List<Segment> segments, string gpxDirectory)
        {
            Segments = segments.ToImmutableList();
            Step = step;
            GpxDirectory = gpxDirectory;
        }

        public ImmutableList<Segment> Segments { get; }
        public int Step { get; }
        public string GpxDirectory { get; }

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
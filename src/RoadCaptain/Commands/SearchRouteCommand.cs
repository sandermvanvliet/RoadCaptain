// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Collections.Generic;
using System.Collections.Immutable;

namespace RoadCaptain.Commands
{
    public class SearchRouteCommand
    {
        public ImmutableArray<string> Repositories { get; } = ImmutableArray<string>.Empty;
        public RetrieveRepositoriesIntent Intent { get; } = RetrieveRepositoriesIntent.Unknown;
        public string? World { get; }
        public string? Creator { get; }
        public string? Name { get; }
        public string? ZwiftRouteName { get; }
        public int? MinDistance { get; }
        public int? MaxDistance { get; }
        public int? MinAscent { get; }
        public int? MaxAscent { get; }
        public int? MinDescent { get; }
        public int? MaxDescent { get; }
        public bool? IsLoop { get; }
        public string[]? KomSegments { get; }
        public string[]? SprintSegments { get; }

        public SearchRouteCommand(
            string repository,
            string? world = null,
            string? creator = null,
            string? name = null,
            string? zwiftRouteName = null,
            int? minDistance = null,
            int? maxDistance = null,
            int? minAscent = null,
            int? maxAscent = null,
            int? minDescent = null,
            int? maxDescent = null,
            bool? isLoop = null,
            string[]? komSegments = null,
            string[]? sprintSegments = null)
         : this(new [] { repository }, world, creator, name, zwiftRouteName, minDistance, maxDistance, minAscent, maxAscent, minDescent, maxDescent, isLoop, komSegments, sprintSegments)
        {
        }

        public SearchRouteCommand(
            IEnumerable<string> repositories,
            string? world = null,
            string? creator = null,
            string? name = null,
            string? zwiftRouteName = null,
            int? minDistance = null,
            int? maxDistance = null,
            int? minAscent = null,
            int? maxAscent = null,
            int? minDescent = null,
            int? maxDescent = null,
            bool? isLoop = null,
            string[]? komSegments = null,
            string[]? sprintSegments = null)
        {
            Repositories = repositories.ToImmutableArray();
            World = world;
            Creator = creator;
            Name = name;
            ZwiftRouteName = zwiftRouteName;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
            MinAscent = minAscent;
            MaxAscent = maxAscent;
            MinDescent = minDescent;
            MaxDescent = maxDescent;
            IsLoop = isLoop;
            KomSegments = komSegments;
            SprintSegments = sprintSegments;
        }

        public SearchRouteCommand(
            RetrieveRepositoriesIntent intent,
            string? world = null,
            string? creator = null,
            string? name = null,
            string? zwiftRouteName = null,
            int? minDistance = null,
            int? maxDistance = null,
            int? minAscent = null,
            int? maxAscent = null,
            int? minDescent = null,
            int? maxDescent = null,
            bool? isLoop = null,
            string[]? komSegments = null,
            string[]? sprintSegments = null)
        {
            Intent = intent;
            World = world;
            Creator = creator;
            Name = name;
            ZwiftRouteName = zwiftRouteName;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
            MinAscent = minAscent;
            MaxAscent = maxAscent;
            MinDescent = minDescent;
            MaxDescent = maxDescent;
            IsLoop = isLoop;
            KomSegments = komSegments;
            SprintSegments = sprintSegments;
        }
    }
}

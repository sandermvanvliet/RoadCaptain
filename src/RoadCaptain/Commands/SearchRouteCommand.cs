namespace RoadCaptain.Commands
{
    public class SearchRouteCommand
    {
        public string Repository { get; }
        public string? World { get; }
        public string? Creator { get; }
        public string? Name { get; }
        public string? ZwiftRouteName { get; }
        public decimal? MinDistance { get; }
        public decimal? MaxDistance { get; }
        public decimal? MinAscent { get; }
        public decimal? MaxAscent { get; }
        public decimal? MinDescent { get; }
        public decimal? MaxDescent { get; }
        public bool? IsLoop { get; }
        public string[]? KomSegments { get; }
        public string[]? SprintSegments { get; }

        public SearchRouteCommand(string repository,
            string? world = null,
            string? creator = null,
            string? name = null,
            string? zwiftRouteName = null,
            decimal? minDistance = null,
            decimal? maxDistance = null,
            decimal? minAscent = null,
            decimal? maxAscent = null,
            decimal? minDescent = null,
            decimal? maxDescent = null,
            bool? isLoop = null,
            string[]? komSegments = null,
            string[]? sprintSegments = null)
        {
            Repository = repository;
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
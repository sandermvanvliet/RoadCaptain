namespace RoadCaptain.Commands
{
    public class SearchRouteCommand
    {
        public string Repository { get; }

        public SearchRouteCommand(string repository)
        {
            Repository = repository;
        }
    }
}
using RoadCaptain.UseCases;

namespace RoadCaptain.Commands
{
    public class RetrieveRepositoryNameCommand
    {
        public RetrieveRepositoryNameCommand(RetrieveRepositoriesIntent intent)
        {
            Intent = intent;
        }

        public RetrieveRepositoriesIntent Intent { get; }
    }
}
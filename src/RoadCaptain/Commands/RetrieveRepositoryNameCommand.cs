// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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

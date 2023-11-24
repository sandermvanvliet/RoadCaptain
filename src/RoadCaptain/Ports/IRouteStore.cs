// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Threading.Tasks;

namespace RoadCaptain.Ports
{
    public interface IRouteStore
    {
        PlannedRoute LoadFrom(string path);
        Task<Uri> StoreAsync(PlannedRoute route, string path);
    }
}


// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Security.Claims;
using RoadCaptain.App.Web.Adapters.EntityFramework;

namespace RoadCaptain.App.Web.Ports
{
    public interface IUserStore
    {
        User? GetOrCreate(ClaimsPrincipal principal);
        User? GetByName(string name);
    }
}

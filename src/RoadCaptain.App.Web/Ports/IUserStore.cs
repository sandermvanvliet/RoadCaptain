using System.Security.Claims;
using RoadCaptain.App.Web.Adapters.EntityFramework;

namespace RoadCaptain.App.Web.Ports
{
    public interface IUserStore
    {
        User? GetOrCreate(ClaimsPrincipal principal);
    }
}
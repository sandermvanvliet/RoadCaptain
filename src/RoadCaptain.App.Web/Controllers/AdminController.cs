using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadCaptain.App.Web.Ports;

namespace RoadCaptain.App.Web.Controllers
{
    [ApiController]
    [Route("/2023-01/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IRouteStore _routeStore;

        public AdminController(IRouteStore routeStore)
        {
            _routeStore = routeStore;
        }

        [HttpGet("duplicates-routes", Name = "FindDuplicateRoutes")]
        [Authorize(Policy = "AdministratorPolicy")]
        public IActionResult FindDuplicateRoutes()
        {
            var duplicates = _routeStore.FindDuplicates();

            var result = duplicates
                .Select(x => new
                {
                    Hash = x.Key,
                    Ids = x.Value.Select(id => Url.Action("GetRouteById", "Routes", new { id = id }))
                });

            return Ok(result);
        }
    }
}
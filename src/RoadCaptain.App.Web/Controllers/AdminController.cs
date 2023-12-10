using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadCaptain.App.Web.Commands;
using RoadCaptain.App.Web.Ports;
using RoadCaptain.App.Web.UseCases;

namespace RoadCaptain.App.Web.Controllers
{
    [ApiController]
    [Route("/2023-01/admin")]
    [Authorize(Policy = "AdministratorPolicy")]
    public class AdminController : ControllerBase
    {
        private readonly IRouteStore _routeStore;
        private readonly RecalculateHashes _recalculateHashesUseCase;

        public AdminController(IRouteStore routeStore, RecalculateHashes recalculateHashesUseCase)
        {
            _routeStore = routeStore;
            _recalculateHashesUseCase = recalculateHashesUseCase;
        }

        [HttpGet("routes/duplicates", Name = "FindDuplicateRoutes")]
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

        [HttpPost("routes/recalculate-hashes", Name = "RecalculateHashes")]
        public IActionResult RecalculateHashes()
        {
            var log = _recalculateHashesUseCase.Execute(new RecalculateHashesCommand(true));

            return Ok(log);
        }
    }
}